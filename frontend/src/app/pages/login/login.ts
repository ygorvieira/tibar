import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { LoginRequest } from '../../models/auth';

@Component({
  selector: 'app-login',
  imports: [FormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class Login {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  model: LoginRequest = { email: '', password: '' };
  error = '';

  submit(): void {
    this.error = '';
    this.auth.login(this.model).subscribe({
      next: () => this.router.navigate(['/dashboard']),
      error: (err) => {
        const body = err.error;
        if (body?.errors) {
          this.error = Array.isArray(body.errors)
            ? body.errors[0]
            : (Object.values(body.errors as Record<string, string[]>)[0]?.[0] ?? 'E-mail ou senha inválidos.');
        } else {
          this.error = 'E-mail ou senha inválidos.';
        }
      }
    });
  }
}
