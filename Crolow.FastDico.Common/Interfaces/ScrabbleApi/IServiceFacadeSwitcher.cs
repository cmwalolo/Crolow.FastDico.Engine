using Crolow.FastDico.Common.Interfaces.ScrabbleApi;

namespace Crolow.TopMachine.Core.Client.Facades
{
    public enum FacadeMode
    {
        Local = 0,
        Remote = 1,
        Server = 2
    }

    public interface IServiceFacadeSwitcher
    {
        FacadeMode Mode { get; set; }

        IServiceFacade Current { get; }

        void SwitchFacade(FacadeMode mode);
    }
}

