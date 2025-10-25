using Crolow.FastDico.Common.Interfaces.Dictionaries;
using Crolow.FastDico.Dicos;
using Crolow.FastDico.Utils;
using System.IO.Compression;
namespace Crolow.FastDico.Base;

public class BaseDictionary : IBaseDictionary
{
    public ILetterNode Root { get; private set; }

    public ILetterNode RootBuild { get; private set; }


    public int BuildNodeId { get; set; }

    private Dictionary<string, ILetterNode> nodeCache;
    protected ITilesUtils tilesUtils;

    public BaseDictionary(ITilesUtils tilesUtils)
    {
        Root = new LetterNode();
        nodeCache = new Dictionary<string, ILetterNode>();
        this.tilesUtils = tilesUtils;
    }

    public virtual void Insert(string word)
    {
    }


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
    /// Save to File save the dictionary on disk
    /// </summary>
    /// <param name="filePath"></param>
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
    /// Save to File save the dictionary on disk
    /// </summary>
    /// <param name="filePath"></param>
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
