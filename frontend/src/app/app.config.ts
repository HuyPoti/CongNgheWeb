import { ApplicationConfig, provideBrowserGlobalErrorListeners, LOCALE_ID } from '@angular/core';
import { provideRouter, withInMemoryScrolling } from '@angular/router';
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { provideClientHydration, withEventReplay } from '@angular/platform-browser';
import { CurrencyPipe, DatePipe } from '@angular/common';
// --- PHẦN THÊM MỚI TỪ AUTH.TXT ---
import {
  GoogleLoginProvider,
  SOCIAL_AUTH_CONFIG,
  SocialAuthService,
  SocialAuthServiceConfig,
} from '@abacritt/angularx-social-login';
import { jwtInterceptor } from './core/interceptors/jwt.interceptor';

import { routes } from './app.routes';

import { environment } from '../environments/environment';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes, withInMemoryScrolling({ scrollPositionRestoration: 'enabled' })),
    { provide: LOCALE_ID, useValue: 'vi' },
    CurrencyPipe,
    DatePipe,

    provideClientHydration(withEventReplay()),
    provideHttpClient(withFetch(), withInterceptors([jwtInterceptor])),
    SocialAuthService,
    {
      provide: SOCIAL_AUTH_CONFIG,
      useValue: {
        autoLogin: false,
        providers: [
          {
            id: GoogleLoginProvider.PROVIDER_ID,
            provider: new GoogleLoginProvider(environment.googleClientId, {
              prompt: 'select_account',
            }),
          },
        ],
        onError: (err: unknown) => console.error(err),
      } as SocialAuthServiceConfig,
    },
  ],
};
