using Assets.Core;

namespace Assets.Scripts.Interfaces
{
    interface IMainUIController
    {
        void HandleInterfaceMoveCardRequest(LineIndicator fromLine, int fromSlotNumber, LineIndicator targetLine, int targetSlotNumber);

        void HandleSingleTargetSelected();
        
        void HandleEndTurnAction();
    }
}
