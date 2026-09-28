(() => {
    function mountLogin() {
        const topbar = document.querySelector(".swagger-ui .topbar .topbar-wrapper");
        if (!topbar || document.getElementById("lash-swagger-login")) {
            return Boolean(topbar);
        }

        const openButton = document.createElement("button");
        openButton.id = "lash-swagger-login";
        openButton.className = "lash-swagger-login-button";
        openButton.type = "button";
        openButton.textContent = "Войти";
        topbar.append(openButton);

        const dialog = document.createElement("dialog");
        dialog.className = "lash-swagger-login-dialog";
        dialog.innerHTML = `
            <form method="dialog" class="lash-swagger-login-form">
                <h2>Вход в Lash API</h2>
                <label>Email<input name="email" type="email" autocomplete="username" required></label>
                <label>Пароль<input name="password" type="password" autocomplete="current-password" required></label>
                <p class="lash-swagger-login-error" role="alert" aria-live="polite"></p>
                <div class="lash-swagger-login-actions">
                    <button type="button" class="lash-swagger-login-cancel">Отмена</button>
                    <button type="submit" class="lash-swagger-login-submit">Войти</button>
                </div>
            </form>`;
        document.body.append(dialog);

        const form = dialog.querySelector("form");
        const email = form.elements.namedItem("email");
        const password = form.elements.namedItem("password");
        const error = dialog.querySelector(".lash-swagger-login-error");
        const submit = dialog.querySelector(".lash-swagger-login-submit");

        openButton.addEventListener("click", () => {
            error.textContent = "";
            dialog.showModal();
            email.focus();
        });
        dialog.querySelector(".lash-swagger-login-cancel").addEventListener("click", () => dialog.close());
        dialog.addEventListener("close", () => {
            password.value = "";
            error.textContent = "";
        });
        form.addEventListener("submit", async event => {
            event.preventDefault();
            error.textContent = "";
            submit.disabled = true;
            const loginUrl = new URL("../api/v1/auth/login", window.location.href);

            try {
                const response = await fetch(loginUrl, {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({ email: email.value, password: password.value }),
                    cache: "no-store"
                });
                if (!response.ok) {
                    error.textContent = response.status === 429
                        ? "Слишком много попыток. Повторите позже."
                        : "Не удалось войти. Проверьте email, пароль и подтверждение почты.";
                    return;
                }

                const result = await response.json();
                const accessToken = result.data?.accessToken;
                if (!accessToken || !window.ui?.preauthorizeApiKey) {
                    error.textContent = "Не удалось установить токен в Swagger UI.";
                    return;
                }

                window.ui.preauthorizeApiKey("bearer", accessToken);
                openButton.textContent = "Сменить пользователя";
                dialog.close();
            } catch {
                error.textContent = "Не удалось связаться с API.";
            } finally {
                password.value = "";
                submit.disabled = false;
            }
        });

        return true;
    }

    document.addEventListener("DOMContentLoaded", () => {
        if (mountLogin()) {
            return;
        }

        const observer = new MutationObserver(() => {
            if (mountLogin()) {
                observer.disconnect();
            }
        });
        observer.observe(document.body, { childList: true, subtree: true });
    });
})();
