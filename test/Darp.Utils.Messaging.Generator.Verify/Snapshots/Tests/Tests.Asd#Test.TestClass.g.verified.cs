//HintName: Test.TestClass.g.cs
namespace Test
{
    partial class TestClass : global::Darp.Utils.Messaging.IMessageSinkProvider
    {
        private ___MessageSink? _;

        /// <inheritdoc />
        public global::Darp.Utils.Messaging.IMessageSink GetMessageSink()
        {
            _eventReceiverProxy ??= new ___MessageSink(this);
            return _eventReceiverProxy;
        }

        private sealed class ___MessageSink(TestClass parent)
            : global::Darp.Utils.Messaging.IMessageSink<int>,
                global::Darp.Utils.Messaging.IMessageSink<ReadOnlySpan<byte>>,
                global::Darp.Utils.Messaging.IAnyMessageSink
        {
            private readonly TestClass _parent = parent;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Publish(in int message)
            {
                _parent.OnInt(message);
                OnIntStatic(message);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Publish(in global::System.ReadOnlySpan<byte> message)
            {
                _parent.OnSpan(message);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Publish<T>(in T message)
                where T : allows ref struct
            {
                _parent.Any(message);
            }
        }
    }
}
