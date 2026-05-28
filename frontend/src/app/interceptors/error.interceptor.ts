import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { NotificationService } from '../services/notification.service';

function extractErrors(err: any): string[] {
  const body = err.error;
  if (!body) return [];

  if (typeof body === 'string') return [body];

  if (body.errors) {
    if (Array.isArray(body.errors)) return body.errors;

    if (typeof body.errors === 'object') {
      return Object.values(body.errors).flatMap(v => Array.isArray(v) ? v : [v]);
    }
  }

  if (body.title) return [body.title];

  return [];
}

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
      } else if (err.status === 0) {
        notifications.error('Erro de rede. Verifique sua conexão.');
      } else {
        const msgs = extractErrors(err);
        if (msgs.length) {
          msgs.forEach(m => notifications.error(m));
        } else {
          notifications.error('Ocorreu um erro inesperado.');
        }
      }

      return throwError(() => err);
    })
  );
};
