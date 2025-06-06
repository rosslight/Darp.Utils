import mirrorsharp from './node_modules/mirrorsharp-codemirror-6-preview';
import {EditorState} from "@codemirror/state"
import {lineNumbers} from "@codemirror/view"

const ms = mirrorsharp(document.getElementById('editor-container'), {
    serviceUrl: window.location.href.replace(/^http(s?:\/\/[^/]+).*$/i, 'ws$1/mirrorsharp'),
    language: "C#",
    text: "// Enter code here!",
    serverOptions: { 'x-mode': "script" },
    codeMirror: {
        extensions: [
            lineNumbers(),
            EditorState.allowMultipleSelections.of(true),
        ]
    }
});

// Ensure editor resizes when window resizes
window.addEventListener('resize', () => {
    // Force editor to update layout
    setTimeout(() => {
        const editorView = ms.getCodeMirror();
        if (editorView) {
            editorView.requestMeasure();
        }
    }, 0);
});

const setTheme = (theme) => {
    ms.setTheme(theme)
}

window.setTheme = setTheme;
