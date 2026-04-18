using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace BT
{
    public enum Output
    {
        Running,
        Failure,
        Success
    }

    public interface INode
    {
        public IReadOnlyCollection<INode> Children { get; }
        public Output Run();
    }

    public class BehaviorTree
    {
        public readonly INode Root;
        
        private readonly List<ITask> m_Tasks;
        private readonly HashSet<ITask> m_TasksRunThisFrame = new();

        public BehaviorTree(INode root)
        {
            Root = root;
            m_Tasks = RegisterTasks();
        }

        public Output Run()
        {
            m_TasksRunThisFrame.Clear();
            Output output = Root.Run();
            HandleInterruptedTasks();
            return output;
        }

        private List<ITask> RegisterTasks()
        {
            List<ITask> tasks = new();
            TraverseTree(Root, node =>
            {
                if (node is TaskAction taskAction)
                {
                    tasks.Add(taskAction.Task);
                    taskAction.OnTaskRun += OnTaskRun;
                }
            });
            return tasks;
        }

        private void OnTaskRun(ITask task)
        {
            m_TasksRunThisFrame.Add(task);
        }

        private void HandleInterruptedTasks()
        {
            foreach(ITask task in m_Tasks)
            {
                if(task.Running && !m_TasksRunThisFrame.Contains(task))
                {
                    task.Running = false;
                    task.OnInterrupted();
                }
            }
        }

        private void TraverseTree(INode current, Action<INode> action)
        {
            if (current is null)
                return;
            if(current.Children is { Count: > 0})
            {
                foreach(var child in current.Children)
                {
                    TraverseTree(child, action);
                }
            }
            action.Invoke(current);
        }
    }

    public class Sequencer : INode
    {
        public IReadOnlyCollection<INode> Children => m_Children;
        private readonly INode[] m_Children;
        private int m_Active;
        public Sequencer(params INode[] children)
        {
            Assert.IsTrue(children is { Length: > 0 }, "Control node must have at least one child");
            m_Children = children;
            m_Active = 0;
        }
        public Output Run()
        {
            for (int i = m_Active; i < m_Children.Length; i++)
            {
                Output output = m_Children[i].Run();

                if (output is Output.Running)
                {
                    m_Active = i;
                    return Output.Running;
                }

                if (output is Output.Failure)
                {
                    m_Active = 0;
                    return Output.Failure;
                }
            }
            m_Active = 0;
            return Output.Success;
        }
    }

    public class ReactiveSequencer : INode
    {
        public IReadOnlyCollection<INode> Children => m_Children;
        private readonly INode[] m_Children;
        public ReactiveSequencer(params INode[] children)
        {
            Assert.IsTrue(children is { Length: > 0 }, "Control node must have at least one child");
            m_Children = children;
        }
        public Output Run()
        {
            for (int i = 0; i < m_Children.Length; i++)
            {
                Output output = m_Children[i].Run();
                if (output is not Output.Success)
                    return output;
            }
            return Output.Success;
        }
    }

    public class Selector : INode
    {
        public IReadOnlyCollection<INode> Children => m_Children;
        private readonly INode[] m_Children;
        private int m_Active;
        public Selector(params INode[] children)
        {
            Assert.IsTrue(children is { Length: > 0 }, "Control node must have at least one child");
            m_Children = children;
            m_Active = 0;
        }
        public Output Run()
        {
            for (int i = m_Active; i < m_Children.Length; i++)
            {
                Output output = m_Children[i].Run();

                if (output is Output.Running)
                {
                    m_Active = i;
                    return Output.Running;
                }

                if (output is Output.Success)
                {
                    m_Active = 0;
                    return Output.Success;
                }
            }
            m_Active = 0;
            return Output.Failure;
        }
    }

    public class ReactiveSelector : INode
    {
        public IReadOnlyCollection<INode> Children => m_Children;
        private readonly INode[] m_Children;
        public ReactiveSelector(params INode[] children)
        {
            Assert.IsTrue(children is { Length: > 0 }, "Control node must have at least one child");
            m_Children = children;
        }
        public Output Run()
        {
            for (int i = 0; i < m_Children.Length; i++)
            {
                Output output = m_Children[i].Run();
                if (output is not Output.Failure)
                    return output;
            }
            return Output.Failure;
        }
    }

    public class Loop : INode
    {
        public IReadOnlyCollection<INode> Children => new[] { m_Child };
        private readonly INode m_Child;
        private readonly Type m_Type;
        private int m_RemainingTimes;
        public Loop(INode child, Type type, int times = -1)
        {
            m_Child = child;
            m_Type = type;
            m_RemainingTimes = times;
            Assert.IsFalse(m_Type is Type.FixedTimes && m_RemainingTimes <= 0, "Minimum 1 fixed execution for a Fixed Times Loop node.");
        }
        public Output Run()
        {
            Output output = m_Child.Run();
            return m_Type switch
            {
                Type.UntilFailure => output is Output.Failure ? Output.Success : Output.Running,
                Type.UntilSuccess => output is Output.Success ? Output.Success : Output.Running,
                Type.FixedTimes => --m_RemainingTimes == 0 ? Output.Success : Output.Running,
                _ => Output.Running,
            };
        }
        public enum Type
        {
            Infinite,
            UntilFailure,
            UntilSuccess,
            FixedTimes
        }
    }

    public class Condition : INode
    {
        public IReadOnlyCollection<INode> Children => null;
        public readonly Func<bool> m_Condition;
        public Condition(Func<bool> condition) => m_Condition = condition;
        public Output Run() => m_Condition.Invoke() ? Output.Success : Output.Failure;
    }

    public class InstantAction : INode
    {
        public IReadOnlyCollection<INode> Children => null;
        public readonly Func<Output> m_Action;
        public InstantAction(Func<Output> action) => m_Action = action;
        public InstantAction(Action action) => m_Action = () => { action.Invoke(); return Output.Success; };
        public Output Run() => m_Action.Invoke();
    }

    public interface ITask
    {
        public bool Running { get; set; }
        public void Start();
        public Output Run();
        public void End(Output output);
        public void OnInterrupted();
    }

    public class TaskAction : INode
    {
        public IReadOnlyCollection<INode> Children => null;
        public readonly ITask Task;
        public event Action<ITask> OnTaskRun;
        public TaskAction(ITask task) { Task = task; }
        public Output Run()
        {
            if(!Task.Running)
            {
                Task.Running = true;
                Task.Start();
            }
            Output output = Task.Run();
            if(output is not Output.Running)
            {
                Task.Running = false;
                Task.End(output);
            }
            OnTaskRun?.Invoke(Task);
            return output;
        }
    }

    public class Inverter : INode
    {
        public IReadOnlyCollection<INode> Children => new[] { m_Child };
        private readonly INode m_Child;
        public Inverter(INode child) => m_Child = child;
        public Output Run()
        {
            return m_Child.Run() switch
            {
                Output.Failure => Output.Success,
                Output.Success => Output.Failure,
                _ => Output.Running
            };
        }
    }

    public class Succeeder : INode
    {
        public IReadOnlyCollection<INode> Children => new[] { m_Child };
        private readonly INode m_Child;
        public Succeeder(INode child = null) => m_Child = child;
        public Output Run()
        {
            m_Child?.Run();
            return Output.Success;
        }
    }

    public class Failer : INode
    {
        public IReadOnlyCollection<INode> Children => new[] { m_Child };
        private readonly INode m_Child;
        public Failer(INode child = null) => m_Child = child;
        public Output Run()
        {
            m_Child?.Run();
            return Output.Failure;
        }
    }

}