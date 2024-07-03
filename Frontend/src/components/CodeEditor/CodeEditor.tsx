import { ChangeEventHandler, FunctionComponent, useContext, useState } from 'react';
import Editor, { OnChange, OnMount } from '@monaco-editor/react';
import { CodeEditorLang } from '../../types/question';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { Theme, ThemeContext } from '../../context/ThemeContext';
import { LocalizationKey } from '../../localization';

import './CodeEditor.css';

export const defaultCodeEditorFontSize = 20;

const fontSizeOptions = [10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48];

const renderOptions = (options: Array<number | string>) =>
  options.map(option => (
    <option key={option} value={option}>
      {option}
    </option>
  ));

interface CodeEditorProps {
  language: CodeEditorLang;
  languages: CodeEditorLang[];
  readOnly?: boolean;
  value?: string | undefined;
  onMount?: OnMount | undefined;
  onChange?: OnChange | undefined;
  onLanguageChange: (language: CodeEditorLang) => void;
  onFontSizeChange?: (size: number) => void;
}

export const CodeEditor: FunctionComponent<CodeEditorProps> = ({
  language,
  languages,
  readOnly,
  value,
  onMount,
  onChange,
  onLanguageChange,
  onFontSizeChange,
}) => {
  const { themeInUi } = useContext(ThemeContext);
  const localizationCaptions = useLocalizationCaptions();
  const [fontSize, setFontSize] = useState(defaultCodeEditorFontSize);

  const handleFontSizeChange: ChangeEventHandler<HTMLSelectElement> = (event) => {
    const newFontSize = Number(event.target.value);
    setFontSize(newFontSize);
    onFontSizeChange?.(newFontSize);
  };

  const handleLanguageChange: ChangeEventHandler<HTMLSelectElement> = (event) => {
    onLanguageChange(event.target.value as CodeEditorLang);
  };

  return (
    <div className='code-editor'>
      <div className='code-editor-tools'>
        <span>{localizationCaptions[LocalizationKey.Language]}:</span>
        <select className='code-editor-tools-select' value={language} onChange={handleLanguageChange}>
          {renderOptions(languages)}
        </select>
        <span>{localizationCaptions[LocalizationKey.FontSize]}:</span>
        <select className='code-editor-tools-select' value={fontSize} onChange={handleFontSizeChange}>
          {renderOptions(fontSizeOptions)}
        </select>
      </div>
      <Editor
        keepCurrentModel={true}
        options={{
          minimap: { enabled: false },
          fontSize,
          quickSuggestions: false,
          readOnly,
        }}
        language={language}
        theme={themeInUi === Theme.Dark ? 'vs-dark' : 'light'}
        value={value}
        onChange={onChange}
        onMount={onMount}
      />
    </div>
  );
};