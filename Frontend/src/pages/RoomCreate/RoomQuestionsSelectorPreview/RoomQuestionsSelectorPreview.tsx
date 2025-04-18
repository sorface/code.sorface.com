import React, { FunctionComponent, MouseEvent } from 'react';
import { RoomQuestionListItem } from '../../../types/room';
import { useLocalizationCaptions } from '../../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../../localization';
import { Dropdown } from '../../../components/Dropdown/Dropdown';
import { Icon } from '../../Room/components/Icon/Icon';
import { IconNames } from '../../../constants';
import { Typography } from '../../../components/Typography/Typography';
import { Gap } from '../../../components/Gap/Gap';
import { sortRoomQuestion } from '../../../utils/sortRoomQestions';
import { useThemeClassName } from '../../../hooks/useThemeClassName';
import { Theme } from '../../../context/ThemeContext';

interface RoomQuestionsSelectorPreviewProps {
  qestions: RoomQuestionListItem[];
  onRemove: (question: RoomQuestionListItem) => void;
}

export const RoomQuestionsSelectorPreview: FunctionComponent<
  RoomQuestionsSelectorPreviewProps
> = ({ qestions, onRemove }) => {
  const localizationCaptions = useLocalizationCaptions();
  const contentWrapperClassName = useThemeClassName({
    [Theme.Dark]: 'bg-modal-bg',
    [Theme.Light]: 'bg-white',
  });
  const itemClassName = useThemeClassName({
    [Theme.Dark]: 'hover:bg-dark-active',
    [Theme.Light]: 'hover:bg-grey-active',
  });

  const handleRemoveClick =
    (qestion: RoomQuestionListItem) => (e: MouseEvent<HTMLSpanElement>) => {
      e.stopPropagation();
      onRemove(qestion);
    };

  return (
    <div>
      <Dropdown
        useButton
        buttonVariant="invertedActive"
        toggleContent={
          <div className="flex">
            <Typography size="m">
              {localizationCaptions[LocalizationKey.RoomSelectedQuestions]}:
            </Typography>
            <Gap sizeRem={0.25} horizontal />
            <Typography size="m">{qestions.length}</Typography>
          </div>
        }
        toggleClassName="w-[13.75rem] flex justify-between"
        toggleIcon
        contentClassName="overflow-auto max-h-[20rem] translate-x-[-4.25rem] translate-y-[0.25rem]"
      >
        <div
          className={`w-[18.125rem] rounded-[0.75rem] shadow-dark-bold ${contentWrapperClassName}`}
        >
          {qestions.sort(sortRoomQuestion).map((qestion) => (
            <div
              key={qestion.id}
              className={`flex items-center justify-between h-[2.125rem] px-[1rem] cursor-pointer ${itemClassName}`}
            >
              <Typography size="s">{qestion.value}</Typography>
              <span onClick={handleRemoveClick(qestion)} className="text-grey2">
                <Icon size="s" name={IconNames.Trash} />
              </span>
            </div>
          ))}
        </div>
      </Dropdown>
    </div>
  );
};
