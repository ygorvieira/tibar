import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { RegisterRequest } from '../../models/auth';

@Component({
  selector: 'app-register',
  imports: [FormsModule, RouterLink],
  templateUrl: './register.html',
  styleUrl: './register.scss'
})
export class Register {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  model: RegisterRequest = { name: '', email: '', password: '' };
  error = '';

  submit(): void {
    this.error = '';
    this.auth.register(this.model).subscribe({
      next: () => this.router.navigate(['/dashboard']),
      error: (err) => {
        const body = err.error;
        if (body?.errors) {
          this.error = Array.isArray(body.errors)
            ? body.errors[0]
            : (Object.values(body.errors as Record<string, string[]>)[0]?.[0] ?? 'Cadastro falhou. Tente novamente.');
        } else {
          this.error = 'Cadastro falhou. Tente novamente.';
        }
      }
    });
  }
}
