using Assets.Core;
using NUnit.Framework;
using System.Diagnostics;
using System.Threading;

namespace Assets.Editor.CoreAssemblyTests
{
    [TestFixture]
    public class GameLogicIntegrationTests
    {
        AutoResetEvent _autoResetEvent;
        GameLogic _gameLogic;
        GameLogicStatusChangedEventArgs _eventArgs;
        Stopwatch _sw;

        [SetUp]
        public void SetUp()
        {
            _autoResetEvent = new AutoResetEvent(false);
            _gameLogic = new GameLogic(PlayerControl.AI, true);
            _gameLogic.SpawnRandomDeck(PlayerIndicator.Top, true);
            _gameLogic.SpawnRandomDeck(PlayerIndicator.Bot, true);
            GameLogic.GameLogicStatusChangedEventHandler += HandleGameLogicStatusChanged;
            _sw = new Stopwatch();
        }

        [TearDown]
        public void TearDown()
        {
            _autoResetEvent.Dispose();
            _autoResetEvent = null;
            _gameLogic = null;
            GameLogic.GameLogicStatusChangedEventHandler -= HandleGameLogicStatusChanged;
            _eventArgs = null;
            _sw = null;
        }

        [Test]
        public void PlayGameAndDoNotCrash_IntegrationTest()
        {
            // act
            _gameLogic.StartNextTurn();

            // assert
            bool flag = _autoResetEvent.WaitOne(500);
            Assert.IsTrue(flag); // wait until event comes
            Assert.AreEqual(_eventArgs.MessageType, GameLogicMessageType.GameOver);
        }

        [Test]
        public void IsFastEnough_IntegrationTest()
        {
            // act
            _sw.Start();
            _gameLogic.StartNextTurn();

            // assert
            bool flag = _autoResetEvent.WaitOne(500);
            Assert.IsTrue(flag); // wait until event comes
            Assert.AreEqual(_eventArgs.MessageType, GameLogicMessageType.GameOver);
            Assert.Less(_sw.ElapsedMilliseconds, 50);
        }

        void HandleGameLogicStatusChanged(object sender, GameLogicStatusChangedEventArgs eventArgs)
        {
            _eventArgs = eventArgs;

            // wait until GameOver event
            if (_eventArgs.MessageType == GameLogicMessageType.GameOver)
            {
                _autoResetEvent.Set();
                _sw.Stop();
            }
            else
                _gameLogic.ReturnControl(); // imitate communication
        }
    }
}
