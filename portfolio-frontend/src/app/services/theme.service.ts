import { Inject, Injectable, PLATFORM_ID, signal } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

export type Theme = 'light' | 'dark';

const THEME_STORAGE_KEY = 'theme';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  readonly theme = signal<Theme>('light');
  private isBrowser: boolean;
  private mediaQuery?: MediaQueryList;

  constructor(@Inject(PLATFORM_ID) private platformId: Object) {
    this.isBrowser = isPlatformBrowser(this.platformId);

    if (!this.isBrowser) {
      return;
    }

    this.mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
    const savedTheme = localStorage.getItem(THEME_STORAGE_KEY) as Theme | null;
    const initialTheme: Theme = savedTheme ?? (this.mediaQuery.matches ? 'dark' : 'light');
    this.applyTheme(initialTheme);

    // Follow the OS setting live, but only while the user hasn't picked a theme explicitly.
    this.mediaQuery.addEventListener('change', (event) => {
      if (!localStorage.getItem(THEME_STORAGE_KEY)) {
        this.applyTheme(event.matches ? 'dark' : 'light');
      }
    });
  }

  toggleTheme(): void {
    const next: Theme = this.theme() === 'dark' ? 'light' : 'dark';
    this.applyTheme(next);

    if (this.isBrowser) {
      localStorage.setItem(THEME_STORAGE_KEY, next);
    }
  }

  /** Clears the manual override and goes back to following the OS setting. */
  resetToSystemTheme(): void {
    if (!this.isBrowser) {
      return;
    }

    localStorage.removeItem(THEME_STORAGE_KEY);
    this.applyTheme(this.mediaQuery!.matches ? 'dark' : 'light');
  }

  private applyTheme(theme: Theme): void {
    this.theme.set(theme);

    if (this.isBrowser) {
      document.documentElement.setAttribute('data-theme', theme);
    }
  }
}
