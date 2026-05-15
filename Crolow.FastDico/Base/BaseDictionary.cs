using Crolow.FastDico.Common.Interfaces.Dictionaries;
using Crolow.FastDico.Dicos;
using Crolow.FastDico.Utils;
using System.IO.Compression;
namespace Crolow.FastDico.Base;

/// <summary>
/// Represents a letter-node dictionary that supports incremental insertion, construction of an optimized graph with
/// shared subtrees, and GZip-compressed binary serialization and deserialization.  
/// </summary>
/// <remarks>Uses Root for the runtime node graph and RootBuild for the build-time graph. Build resets build
/// state, inserts words, and calls Optimize to order children deterministically and deduplicate equivalent subtrees
/// using computed node signatures and a signature cache. Optimize mutates the graph in-place and is not thread-safe.
/// SaveToFile and WriteToStream serialize the graph with stable integer identifiers using a BinaryWriter wrapped in a
/// GZipStream; SaveToFile overwrites the target file. ReadFromFile and ReadFromStream reconstruct the graph from the
/// same compressed binary format and propagate I/O or format errors. Dispose clears node references and caches to
/// release in-memory structures.</remarks>
public class BaseDictionary : IBaseDictionary
{
    /// <summary>
    /// Gets the root letter node.  
    /// </summary>
    public ILetterNode Root { get; private set; }

    /// <summary>
    /// Gets the root ILetterNode of the constructed letter-node graph. 
    /// </summary>
    /// <remarks>Null if no build has been performed. Value is set privately by the builder.</remarks>
    public ILetterNode RootBuild { get; private set; }


    /// <summary>
    /// Gets or sets the identifier of the build node.  
    /// </summary>
    /// <remarks>Unique identifier assigned to a build node within the build system.</remarks>
    public int BuildNodeId { get; set; }

    private Dictionary<string, ILetterNode> nodeCache;
    protected ITilesUtils tilesUtils;

    public BaseDictionary(ITilesUtils tilesUtils)
    {
        Root = new LetterNode();
        nodeCache = new Dictionary<string, ILetterNode>();
        this.tilesUtils = tilesUtils;
    }

    /// <summary>
    /// Inserts the specified word into the data structure.
    /// </summary>
    /// <param name="word">The word to insert.</param>
    public virtual void Insert(string word)
    {
    }


    /// <summary>
    /// Initializes internal build state and constructs the letter-node structure from the provided words by inserting
    /// each word and optimizing the result.    
    /// </summary>
    /// <remarks>Resets the node cache and build identifier, creates a new root node, inserts each word using
    /// Insert(word), then calls Optimize() to compact the structure.</remarks>
    /// <param name="words">Sequence of words to insert into the build structure.</param>
    public void Build(IEnumerable<string> words)
    {
        nodeCache = new Dictionary<string, ILetterNode>();
        BuildNodeId = 0;
        RootBuild = new LetterNode();

        foreach (var word in words)
        {
            Insert(word);
        }

        Optimize();
    }

    /// <summary>
    /// Optimizes the node graph rooted at RootBuild by ordering each node's children by letter and merging equivalent
    /// subtrees using a signature cache.   
    /// </summary>
    /// <remarks>Performs an in-place breadth-first traversal. For each node, Children are sorted by Letter
    /// and child nodes with identical signatures (as computed by GetNodeSignature) are replaced with shared instances
    /// from nodeCache to deduplicate subtrees. The operation mutates the graph and is not thread-safe.</remarks>
    private void Optimize()
    {
        int processedNodes = 0;

        List<ILetterNode> nodesToProcess = new List<ILetterNode> { RootBuild };
        while (nodesToProcess.Count > 0)
        {
            processedNodes++;
            var currentNode = nodesToProcess[0];
            nodesToProcess.RemoveAt(0);

            currentNode.Children = currentNode.Children.OrderBy(p => p.Letter).ToList();

            for (int x = 0; x < currentNode.Children.Count; x++)
            {
                var node = currentNode.Children[x];
                string nodeSignature = GetNodeSignature(node);

                if (nodeCache.ContainsKey(nodeSignature))
                {
                    currentNode.Children[x] = nodeCache[nodeSignature];
                }
                else
                {
                    nodeCache[nodeSignature] = node;
                    nodesToProcess.Add(node);
                }
            }
        }
    }

    /// <summary>
    /// Generate a deterministic signature for a node and its subtree by combining the node's letter, control flag, and
    /// ordered child signatures.   
    /// </summary>
    /// <remarks>Children are ordered by Letter to ensure deterministic signatures; the method recurses to
    /// include child signatures and includes the Control value to distinguish terminal states.</remarks>
    /// <param name="node">Node whose subtree is represented in the signature.</param>
    /// <returns>A canonical string that uniquely identifies the node and its descendants, including the node's letter, control
    /// flag, and ordered child signatures.</returns>
    private string GetNodeSignature(ILetterNode node)
    {
        // Include the letter in the signature to ensure uniqueness
        var childrenSignatures = node.Children
            .OrderBy(c => c.Letter) // Ensure ordered processing
            .Select(child => GetNodeSignature(child))
            .ToList();

        // Create a combined signature that includes the node's letter and terminal status
        var signature = $"[{node.Letter}{node.Control}-{string.Join(",", childrenSignatures)}]";

        return signature;
    }

    /// <summary>
    /// Serializes the letter-node graph to the specified file using a binary format with GZip compression. 
    /// </summary>
    /// <remarks>Writes a node count header and each node with a stable integer identifier using a BinaryWriter
    /// wrapped in a GZipStream. The file is created with FileMode.Create, so the target is replaced. IO-related exceptions
    /// (for example IOException or UnauthorizedAccessException) may be thrown.</remarks>
    /// <param name="filePath">Path of the output file to create; an existing file is overwritten.</param>
    public void SaveToFile(string filePath)
    {
        using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
        using (GZipStream gzipStream = new GZipStream(fileStream, CompressionMode.Compress))
        using (BinaryWriter writer = new BinaryWriter(gzipStream))
        {
            Dictionary<ILetterNode, int> nodeToId = new Dictionary<ILetterNode, int>();
            int currentId = 0;

            List<ILetterNode> writeOrder = new List<ILetterNode>();
            CollectNodesWithIds(RootBuild, nodeToId, ref currentId, writeOrder);

            writer.Write(writeOrder.Count);

            foreach (var node in writeOrder)
            {
                WriteNodeWithId(node, writer, nodeToId);
            }
        }
    }

    /// <summary>
    /// Writes the node graph as a GZip-compressed binary representation to the specified stream.   
    /// </summary>
    /// <remarks>Data are written using a BinaryWriter over a GZipStream; the node count is written first followed by
    /// each node in a deterministic order. The GZipStream is disposed after writing, which also closes the provided
    /// stream.</remarks>
    /// <param name="stream">The destination stream for the GZip-compressed, binary-serialized nodes.</param>
    /// <returns>The stream that was written to.</returns>
    public Stream WriteToStream(Stream stream)
    {
        using (var outputStream = new MemoryStream())
        using (GZipStream gzipStream = new GZipStream(stream, CompressionMode.Compress))
        using (BinaryWriter writer = new BinaryWriter(gzipStream))
        {
            Dictionary<ILetterNode, int> nodeToId = new Dictionary<ILetterNode, int>();
            int currentId = 0;

            List<ILetterNode> writeOrder = new List<ILetterNode>();
            CollectNodesWithIds(RootBuild, nodeToId, ref currentId, writeOrder);

            writer.Write(writeOrder.Count);

            foreach (var node in writeOrder)
            {
                WriteNodeWithId(node, writer, nodeToId);
            }

            return outputStream;
        }
    }

    /// <summary>
    /// Assigns sequential integer identifiers to nodes during a depth-first traversal and records nodes in write order.    
    /// </summary>
    /// <remarks>Recursively traverses node.Children and skips nodes already present in nodeToId to avoid
    /// revisiting nodes and prevent infinite recursion in graphs with cycles.</remarks>
    /// <param name="node">Node to assign an identifier to if not already mapped.</param>
    /// <param name="nodeToId">Dictionary mapping nodes to assigned identifiers; new entries are added for visited nodes.</param>
    /// <param name="currentId">Reference to the next identifier to assign; incremented after assigning an identifier.</param>
    /// <param name="writeOrder">List that receives nodes in the order they are first assigned an identifier (preorder traversal).</param>
    private void CollectNodesWithIds(ILetterNode node, Dictionary<ILetterNode, int> nodeToId, ref int currentId, List<ILetterNode> writeOrder)
    {
        if (nodeToId.ContainsKey(node))
            return;

        nodeToId[node] = currentId++;
        writeOrder.Add(node);

        foreach (var child in node.Children)
        {
            CollectNodesWithIds(child, nodeToId, ref currentId, writeOrder);
        }
    }

    /// <summary>
    /// Writes the node's control value, the number of children, and for each child writes the child's letter followed
    /// by its assigned integer ID to the provided BinaryWriter.    
    /// </summary>
    /// <remarks>The child count is written as a single byte; nodeToId must contain an entry for every
    /// child.</remarks>
    /// <param name="node">The node whose control value and children are written.</param>
    /// <param name="writer">The BinaryWriter to which the node data is written.</param>
    /// <param name="nodeToId">Mapping from child nodes to their assigned integer identifiers used when writing child references.</param>
    private void WriteNodeWithId(ILetterNode node, BinaryWriter writer, Dictionary<ILetterNode, int> nodeToId)
    {
        writer.Write(node.Control);
        writer.Write((byte)node.Children.Count);

        foreach (var child in node.Children)
        {
            writer.Write(child.Letter); // Write the child letter
            writer.Write(nodeToId[child]);   // Write the child ID
        }
    }

    /// <summary>
    /// Reads a GZip-compressed binary file containing serialized letter nodes and reconstructs the node graph, setting
    /// Root to the first node. 
    /// </summary>
    /// <remarks>Opens the file with FileShare.Read, expects an Int32 node count followed by node data in a
    /// GZip-compressed binary format. Allocates the required LetterNode instances, populates them via ReadNodeWithId,
    /// and overwrites the existing Root. I/O, compression, or format errors are propagated.</remarks>
    /// <param name="filePath">Path of the GZip-compressed binary file that contains the serialized nodes.</param>
    public void ReadFromFile(string filePath)
    {
        Root = new LetterNode();

        using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        using (GZipStream gzipStream = new GZipStream(fileStream, CompressionMode.Decompress))
        using (BinaryReader reader = new BinaryReader(gzipStream))
        {
            int nodeCount = reader.ReadInt32();
            List<ILetterNode> nodeList = new List<ILetterNode>(nodeCount);

            for (int i = 0; i < nodeCount; i++)
            {
                nodeList.Add(new LetterNode());
            }

            for (int i = 0; i < nodeCount; i++)
            {
                ReadNodeWithId(nodeList[i], reader, nodeList);
            }

            Root = nodeList[0];
        }
    }

    /// <summary>
    /// Populate the given node by reading its control byte and child references from a binary stream.  
    /// </summary>
    /// <remarks>Reads a control byte into node.Control, reads the number of children, then for each child
    /// reads a letter and an Int32 id, assigns the letter to the resolved child node, and adds it to node.Children.
    /// Assumes reader is positioned correctly and nodeList contains all referenced ids; may throw on out-of-range ids
    /// or truncated stream.</remarks>
    /// <param name="node">Node to populate with the control value and resolved child nodes.</param>
    /// <param name="reader">BinaryReader positioned at the serialized node data; reads control byte, child count, child letters, and child
    /// identifiers.</param>
    /// <param name="nodeList">List of nodes indexed by identifier used to resolve and link child nodes.</param>
    private void ReadNodeWithId(ILetterNode node, BinaryReader reader, List<ILetterNode> nodeList)
    {
        node.Control = reader.ReadByte();
        int childrenCount = reader.ReadByte();

        for (int i = 0; i < childrenCount; i++)
        {
            byte childLetter = reader.ReadByte();
            int childId = reader.ReadInt32();

            ILetterNode childNode = nodeList[childId];
            childNode.Letter = childLetter;  // Set the letter for the child node
            node.Children.Add(childNode);
        }
    }

    /// <summary>
    /// Reads a GZip-compressed binary representation of a letter-node graph from the specified stream and reconstructs
    /// the graph into the Root property.   
    /// </summary>
    /// <remarks>The method decompresses the entire payload into memory before deserialization and therefore
    /// may allocate memory proportional to the uncompressed data. It reads an Int32 node count, creates node instances,
    /// and populates them via ReadNodeWithId; Root is set to the first node. Exceptions are thrown for invalid or
    /// incomplete data or decompression failures.</remarks>
    /// <param name="stream">A readable Stream containing GZip-compressed binary data for the serialized letter-node graph. The stream is
    /// consumed and will be closed by this method.</param>
    public void ReadFromStream(Stream stream)
    {
        Root = new LetterNode();

        using (MemoryStream memoryStream = new MemoryStream())
        {
            using (GZipStream gzipStream = new GZipStream(stream, CompressionMode.Decompress))
            {
                gzipStream.CopyTo(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
            }


            using (BinaryReader reader = new BinaryReader(memoryStream))
            {
                int nodeCount = reader.ReadInt32();
                List<ILetterNode> nodeList = new List<ILetterNode>(nodeCount);

                for (int i = 0; i < nodeCount; i++)
                {
                    nodeList.Add(new LetterNode());
                }

                for (int i = 0; i < nodeCount; i++)
                {
                    ReadNodeWithId(nodeList[i], reader, nodeList);
                }
                Root = nodeList[0];
            }
        }
    }

    public void Dispose()
    {
        Root?.Children.Clear();
        RootBuild?.Children.Clear();
        Root = null;
        RootBuild = null;
        nodeCache?.Clear();
        nodeCache = null;
    }
}
