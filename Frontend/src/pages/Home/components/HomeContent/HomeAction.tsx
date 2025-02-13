import React, { FunctionComponent } from 'react';
import { VITE_BACKEND_URL } from '../../../../config';
import { LocalizationKey } from '../../../../localization';
import { useLocalizationCaptions } from '../../../../hooks/useLocalizationCaptions';
import { Button } from '../../../../components/Button/Button';

export const HomeAction: FunctionComponent = () => {
  return (
    <a
      href={`${VITE_BACKEND_URL}/login/sorface?redirectUri=${encodeURIComponent(window.location.href)}`}
    >
      <Button variant="active">
        {useLocalizationCaptions()[LocalizationKey.Login]}
      </Button>
    </a>
  );
};
