using System.Collections.Generic;
using System.Reflection;

namespace RedArrow.Argo.Linq.Node
{
    internal abstract class NodeBase
    {

        public NodeBase Target { get; }

        public static NodeBase Null { get; } = new NullNode();

        public static NodeBase DirectValue { get; } = new DirectValueNode();

        public bool IsTerminal
        {
            get
            {
                var type = GetType();
                if (!type.GetTypeInfo().IsGenericType) return false;
                if (type.GetGenericTypeDefinition() != typeof(TerminalNode<>)) return false;

                var typeArgument = type.GetTypeInfo().GenericTypeArguments[0];
                return !typeArgument.GetTypeInfo().IsGenericType || typeArgument.GetGenericTypeDefinition() != typeof(IEnumerable<>);
            }
        }

        public bool IsTerminalList
        {
            get
            {
                var type = GetType();
                if (!type.GetTypeInfo().IsGenericType) return false;
                if (type.GetGenericTypeDefinition() != typeof(TerminalNode<>)) return false;

                var typeArgument = type.GetTypeInfo().GenericTypeArguments[0];
                return typeArgument.GetTypeInfo().IsGenericType && typeArgument.GetGenericTypeDefinition() == typeof(IEnumerable<>);
            }
        }

        public bool IsNull => this == Null;

        public bool IsDirectValue => this == DirectValue;

        #region Abstracts

        protected abstract NodeBase ReduceTerminalItem(NodeBase node);
        protected abstract NodeBase ReduceTerminalList(NodeBase node);
        protected abstract NodeBase ReduceDirectValue(object value);

        #endregion
        
        protected NodeBase(NodeBase target)
        {
            Target = target;
        }

        public static NodeBase CreateTerminal<T>(T value)
        {
            return new TerminalNode<T>(value);
        }

        public T GetTerminalValue<T>()
        {
            return ((TerminalNode<T>)this).Value;
        }

        public IEnumerable<T> GetTerminalValues<T>()
        {
            return ((TerminalNode<IEnumerable<T>>)this).Value;
        }

        public NodeBase Reduce(object item)
        {
            var node = this.Target;
            while (true)
            {
                if (node.IsTerminal) return ReduceTerminalItem(node);
                if (node.IsTerminalList) return ReduceTerminalList(node);
                if (node.IsDirectValue) return ReduceDirectValue(item);
                if (node.IsNull) return node;

                node = node.Reduce(item);
            }
        }

        #region TerminalNode

        private class TerminalNode<T> : NodeBase
        {
            public TerminalNode(T value) : base(null)
            {
                Value = value;
            }

            public T Value { get; }

            protected override NodeBase ReduceTerminalItem(NodeBase node)
            {
                return Null;
            }

            protected override NodeBase ReduceTerminalList(NodeBase node)
            {
                return Null;
            }

            protected override NodeBase ReduceDirectValue(object value)
            {
                return Null;
            }
        }

        #endregion

        #region NullNode
        
        private class NullNode : NodeBase
        {
            public NullNode() : base(null) { }

            protected override NodeBase ReduceTerminalItem(NodeBase node)
            {
                return Null;
            }

            protected override NodeBase ReduceTerminalList(NodeBase node)
            {
                return Null;
            }

            protected override NodeBase ReduceDirectValue(object value)
            {
                return Null;
            }
        }

        #endregion

        #region DirectValueNode
        
        private class DirectValueNode : NodeBase
        {
            public DirectValueNode() : base(null) { }

            protected override NodeBase ReduceTerminalItem(NodeBase node)
            {
                return Null;
            }

            protected override NodeBase ReduceTerminalList(NodeBase node)
            {
                return Null;
            }

            protected override NodeBase ReduceDirectValue(object value)
            {
                return Null;
            }
        }

        #endregion
    }
}