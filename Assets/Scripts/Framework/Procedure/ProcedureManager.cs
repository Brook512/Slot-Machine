using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Data;
/*
思路说明：
1. ProcedureManager 是当前阶段唯一需要挂在 Unity 场景中的流程入口。
2. 它负责持有当前 Procedure，并在 Update 中持续驱动当前 Procedure。
3. 它同时负责初始化 GameRuntime，让运行时数据只在一个地方被创建。
4. 后续无论是切到主菜单、游戏中、游戏结束，都是通过 ChangeProcedure 完成切换。
*/

using Framework.Runtime;

namespace Framework.Procedure
{
    public class ProcedureManager : MonoBehaviour
    {
        public ProcedureBase CurrentProcedure { get; private set; }
        public GameRuntime Runtime { get; private set; }

        [SerializeField] private GameConfig gameConfig;
        public GameConfig GameConfig => gameConfig;
        private void Awake()
        {
            Runtime = new GameRuntime();
            Runtime.Initialize();
            Runtime.BindConfig(gameConfig); // 新增
        }

        private void Start()
        {
            ChangeProcedure(new BootstrapProcedure(this));
        }

        private void Update()
        {
            CurrentProcedure?.OnUpdate(Runtime, Time.deltaTime);
        }

        public void ChangeProcedure(ProcedureBase newProcedure)
        {
            if (newProcedure == null)
            {
                Debug.LogError("[ProcedureManager] Cannot change to null procedure.");
                return;
            }

            CurrentProcedure?.OnExit(Runtime);

            CurrentProcedure = newProcedure;

            Debug.Log($"[ProcedureManager] Enter Procedure: {CurrentProcedure.GetType().Name}");
            CurrentProcedure.OnEnter(Runtime);
        }
    }
}
