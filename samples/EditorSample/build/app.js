import mirrorsharp from './node_modules/mirrorsharp-codemirror-6-preview';
import {lineNumbers, EditorView, keymap, highlightSpecialChars, drawSelection, dropCursor, rectangularSelection, crosshairCursor, highlightActiveLineGutter} from "@codemirror/view"
import {indentOnInput, bracketMatching, foldKeymap} from "@codemirror/language"
import {defaultKeymap, historyKeymap} from "@codemirror/commands"
import {searchKeymap} from "@codemirror/search"
import {completionKeymap, closeBrackets, closeBracketsKeymap} from "@codemirror/autocomplete"
import {lintKeymap} from "@codemirror/lint"
import { Transaction, EditorState } from "@codemirror/state";

const contentChangedListener = EditorView.updateListener.of(update => {
    const userTyped = update.transactions.some(tr =>
      tr.annotation(Transaction.userEvent) != null
    );
    if (userTyped && update.docChanged) {
        const txt = update.state.doc.toString();
        logString("docChanged", txt)
        // invoke the .NET callback to be registered on navigation
        if (window.msTextChanged && window.msTextChanged.invoke)
            window.msTextChanged.invoke(JSON.stringify(txt).slice(1, -1));
    }
});

const ms = mirrorsharp(document.getElementById('editor-container'), {
    serviceUrl: window.location.href.replace(/^http(s?:\/\/[^/]+).*$/i, 'ws$1/mirrorsharp'),
    language: "C#",
    text: "",
    codeMirror: {
        extensions: [
            lineNumbers(),
            closeBrackets(),
            contentChangedListener,
            lineNumbers(),
            highlightActiveLineGutter(),
            highlightSpecialChars(),
            drawSelection(),
            dropCursor(),
            EditorState.allowMultipleSelections.of(true),
            indentOnInput(),
            bracketMatching(),
            rectangularSelection(),
            crosshairCursor(),
            keymap.of([
              ...closeBracketsKeymap,
              ...defaultKeymap,
              ...searchKeymap,
              ...historyKeymap,
              ...foldKeymap,
              ...completionKeymap,
              ...lintKeymap
            ])
        ]
    }
});

// Getter and setter to be called from .Net

window.getMsText = () => ms.getText()
window.setMsText = (text) => {
    const currentText = ms.getCodeMirrorView().state.doc.toString()
    if (currentText === text) {
      console.log("Do not set text. State is equal");
      return;
    }
    logString("currentMsText", currentText)
    console.log(`Equal: ${currentText === text}`)
    logString("setMsText", text)
    ms.setText(text)
}
window.getMsLanguage = () => ms.getLanguage()
window.setMsLanguage = (language) => ms.setLanguage(language)

window.setScriptMode = (scriptMode) => ms.setServerOptions({'x-mode': scriptMode ? "script" : "regular"});

window.setMsTheme = (theme) => ms.setTheme(theme);

const ESCAPES = {
  ' ': '[space]',
  '\r': '[cr]',
  '\n': '[lf]',
  '\t': '[tab]',
  '\f': '[form]',
  '\b': '[bs]',
  '\v': '[vt]',
  '\0': '[null]',
  '\\': '[backslash]',
  '"': '[dq]',
  "'": '[sq]',
};

function escapeChar(ch) {
  if (ESCAPES.hasOwnProperty(ch)) {
    return ESCAPES[ch];
  }
  const code = ch.charCodeAt(0);
  // non-printable or non-ASCII
  if (code < 32 || code > 126) {
    return `[0x${code.toString(16).padStart(2, '0')}]`;
  }
  return ch;
}

const logString = (start, text) => {
  if (text === null || text === undefined) {
    console.log(`${start} ${text}`);
    return;
  }
  if (text.length === 0) {
    console.log(`${start} [empty]`);
    return;
  }
  const escaped = Array.from(text, escapeChar).join('');
  console.log(`${start} ${escaped}`);
};
