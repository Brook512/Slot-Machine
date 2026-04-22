/*
思路说明：
1. EventBus 是整个项目的轻量事件总线。
2. 第三步开始，除了流程事件和结果事件，还要支持“UI 请求事件”，例如点击 Spin、点击下注按钮。
3. 事件层只负责广播“发生了什么”，不直接处理业务。
4. RunProcedure、UI 视图、调试组件都通过事件总线解耦协作。
*/

using System;
using System.Collections.Generic;

namespace Framework.Event
{
    public class EventBus
    {
        private readonly Dictionary<Type, Delegate> _eventTable = new();

        public void Subscribe<T>(Action<T> handler) where T : class
        {
            var eventType = typeof(T);

            if (_eventTable.TryGetValue(eventType, out var existingDelegate))
            {
                _eventTable[eventType] = Delegate.Combine(existingDelegate, handler);
            }
            else
            {
                _eventTable[eventType] = handler;
            }
        }

        public void Unsubscribe<T>(Action<T> handler) where T : class
        {
            var eventType = typeof(T);

            if (!_eventTable.TryGetValue(eventType, out var existingDelegate))
                return;

            var newDelegate = Delegate.Remove(existingDelegate, handler);

            if (newDelegate == null)
            {
                _eventTable.Remove(eventType);
            }
            else
            {
                _eventTable[eventType] = newDelegate;
            }
        }

        public void Publish<T>(T eventData) where T : class
        {
            var eventType = typeof(T);

            if (_eventTable.TryGetValue(eventType, out var existingDelegate))
            {
                if (existingDelegate is Action<T> callback)
                {
                    callback.Invoke(eventData);
                }
            }
        }

        public void Clear()
        {
            _eventTable.Clear();
        }
    }

    public sealed class RuntimeInitializedEvent
    {
        public int Level { get; }
        public int Points { get; }
        public int GoalPoints { get; }
        public int SpinsRemaining { get; }

        public RuntimeInitializedEvent(int level, int points, int goalPoints, int spinsRemaining)
        {
            Level = level;
            Points = points;
            GoalPoints = goalPoints;
            SpinsRemaining = spinsRemaining;
        }
    }

    public sealed class RunPhaseChangedEvent
    {
        public Framework.Runtime.RunPhase NewPhase { get; }

        public RunPhaseChangedEvent(Framework.Runtime.RunPhase newPhase)
        {
            NewPhase = newPhase;
        }
    }

    public sealed class PointsChangedEvent
    {
        public int CurrentPoints { get; }

        public PointsChangedEvent(int currentPoints)
        {
            CurrentPoints = currentPoints;
        }
    }

    public sealed class BetChangedEvent
    {
        public int TotalBet { get; }

        public BetChangedEvent(int totalBet)
        {
            TotalBet = totalBet;
        }
    }

    public sealed class SpinCompletedEvent
    {
        public int SectorIndex { get; }
        public string SymbolId { get; }

        public SpinCompletedEvent(int sectorIndex, string symbolId)
        {
            SectorIndex = sectorIndex;
            SymbolId = symbolId;
        }
    }

    public sealed class SettlementCompletedEvent
    {
        public string SymbolId { get; }
        public int TotalBet { get; }
        public int Reward { get; }
        public int CurrentPoints { get; }
        public bool IsWin { get; }

        public SettlementCompletedEvent(string symbolId, int totalBet, int reward, int currentPoints, bool isWin)
        {
            SymbolId = symbolId;
            TotalBet = totalBet;
            Reward = reward;
            CurrentPoints = currentPoints;
            IsWin = isWin;
        }
    }

    public sealed class SpinsChangedEvent
    {
        public int SpinsRemaining { get; }

        public SpinsChangedEvent(int spinsRemaining)
        {
            SpinsRemaining = spinsRemaining;
        }
    }

    public sealed class BetRequestEvent
    {
        public string SymbolId { get; }
        public int Amount { get; }

        public BetRequestEvent(string symbolId, int amount)
        {
            SymbolId = symbolId;
            Amount = amount;
        }
    }

    public sealed class SpinRequestEvent
    {
    }
    public sealed class LevelStartedEvent
    {
        public int Level { get; }
        public int Points { get; }
        public int GoalPoints { get; }
        public int SpinsRemaining { get; }

        public LevelStartedEvent(int level, int points, int goalPoints, int spinsRemaining)
        {
            Level = level;
            Points = points;
            GoalPoints = goalPoints;
            SpinsRemaining = spinsRemaining;
        }
    }

    public sealed class LevelFinishedEvent
    {
        public int Level { get; }
        public bool IsSuccess { get; }
        public int FinalPoints { get; }

        public LevelFinishedEvent(int level, bool isSuccess, int finalPoints)
        {
            Level = level;
            IsSuccess = isSuccess;
            FinalPoints = finalPoints;
        }
    }
    public sealed class WheelSpinAnimationCompletedEvent
    {
        public int SectorIndex { get; }
        public string SymbolId { get; }

        public WheelSpinAnimationCompletedEvent(int sectorIndex, string symbolId)
        {
            SectorIndex = sectorIndex;
            SymbolId = symbolId;
        }
    }

}