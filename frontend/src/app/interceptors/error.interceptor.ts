import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { NotificationService } from '../services/notification.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const auth = inject(AuthService);
  const notifications = inject(NotificationService);

  return next(req).pipe(
    catchError(err => {
      if (err.status === 401) {
        auth.logout();
        router.navigate(['/login']);
        notifications.error('Sessão expirada. Faça login novamente.');
      } else if (err.status === 400 && err.error?.errors) {
        const msgs = Array.isArray(err.error.errors) ? err.error.errors : [err.error.errors];
        msgs.forEach((m: string) => notifications.error(m));
      } else if (err.status === 0) {
        notifications.error('Erro de rede. Verifique sua conexão.');
      } else {
        notifications.error(err.error?.errors?.[0] || 'Ocorreu um erro inesperado.');
      }

      return throwError(() => err);
    })
  );
};
