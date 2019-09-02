using Assets.Core;

namespace Assets.Scripts.Interfaces
{
    interface IMainUIController
    {
        void HandleEndTurnAction();

        void HandleInterfaceMoveCardRequest(LineIndicator fromLine, int fromSlotNumber, LineIndicator targetLine, int targetSlotNumber);
    }
}
