const formLogin = document.getElementById("form-login");
const erroLogin = document.getElementById("login-erro");
const botaoEntrar = document.getElementById("entrar");

// Quem já entrou e abre esta página vai direto para o sistema
api.requisitar("GET", "login").then(() => location.replace("./"), () => {});

formLogin.addEventListener("submit", async evento => {
    evento.preventDefault();
    erroLogin.hidden = true;
    botaoEntrar.disabled = true;
    botaoEntrar.textContent = "Entrando…";

    try {
        await api.requisitar("POST", "login", {
            matricula: formLogin.elements.matricula.value.trim(),
            senha: formLogin.elements.senha.value
        });
        location.replace("./");
    } catch (erro) {
        erroLogin.textContent = erro.message;
        erroLogin.hidden = false;
        botaoEntrar.disabled = false;
        botaoEntrar.textContent = "Entrar";
        formLogin.elements.senha.select();
    }
});

formLogin.elements.matricula.focus();
