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

const setTheme = (theme) => {
    ms.setTheme(theme)
}

window.setTheme = setTheme;

// Getter and setter to be called from .Net

window.setMsText = (text) => ms.setText(text)
window.getMsText = () => ms.getText()

window.setMsLanguage = (language) => ms.setLanguage(language)
window.getMsLanguage = () => ms.getLanguage()

const setMsTheme = (theme) => ms.setTheme(theme);

window.setMsTheme = (theme) => setMsTheme(theme);
