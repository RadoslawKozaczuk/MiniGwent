using Assets.Core;

namespace Assets.Scripts.Interfaces
{
    interface IMainUIController
    {
        void HandleEndTurnAction();

        void HandleInterfaceMoveCardRequest(Line fromLine, int fromSlotNumber, Line targetLine, int targetSlotNumber);
    }
}
