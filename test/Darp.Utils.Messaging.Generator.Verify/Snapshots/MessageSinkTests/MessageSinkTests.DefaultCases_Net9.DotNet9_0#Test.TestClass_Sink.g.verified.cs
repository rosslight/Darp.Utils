﻿//HintName: Test.TestClass_Sink.g.cs
// <auto-generated/>
#nullable enable
#pragma warning disable CS0618 // Suppress obsolete

namespace Test
{
    partial class TestClass : global::Darp.Utils.Messaging.IMessageSinkProvider
    {
        [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.Utils.Messaging.Generator", "GeneratorVersion")]
        [global::System.Obsolete("This field is not intended to be used in use code. Use 'GetMessageSink'")]
        private ___MessageSink? ___lazyMessageSink;

        /// <inheritdoc />
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.Utils.Messaging.Generator", "GeneratorVersion")]
        public global::Darp.Utils.Messaging.IMessageSink GetMessageSink()
        {
            return ___lazyMessageSink ??= new ___MessageSink(this);
        }

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Darp.Utils.Messaging.Generator", "GeneratorVersion")]
        private sealed class ___MessageSink
            : global::Darp.Utils.Messaging.IMessageSink<int>, global::Darp.Utils.Messaging.IMessageSink<global::System.ReadOnlySpan<byte>>, global::Darp.Utils.Messaging.IAnyMessageSink
        {
            private readonly TestClass _parent;

            public ___MessageSink(TestClass parent)
            {
                _parent = parent;
            }

            public void Publish(in int message)
            {
                _parent.OnInt(message);
                OnIntStatic(message);
            }
            public void Publish(in global::System.ReadOnlySpan<byte> message)
            {
                _parent.OnSpan(message);
            }
            public void Publish<T>(in T message)
                where T : allows ref struct
            {
                _parent.OnAny(message);
                OnAnyStatic(message);
            }
        }
    }
}
