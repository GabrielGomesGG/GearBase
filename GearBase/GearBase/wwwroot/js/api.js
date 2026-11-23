class ErroApi extends Error {
    constructor(mensagem, status, detalhes) {
        super(mensagem);
        this.status = status;
        this.detalhes = detalhes;
    }
}

const api = {
    async requisitar(metodo, caminho, corpo) {
        let resposta;
        try {
            resposta = await fetch(`/api/${caminho}`, {
                method: metodo,
                headers: corpo ? { "Content-Type": "application/json" } : {},
                body: corpo ? JSON.stringify(corpo) : undefined
            });
        } catch {
            throw new ErroApi("Não foi possível falar com o servidor. Confira se a API está rodando.", 0);
        }

        /* Sem login (ou com o login vencido): volta para a tela de entrada.
           A promessa não termina porque a página vai ser trocada. */
        if (resposta.status === 401 && !location.pathname.endsWith("login.html")) {
            location.replace("login.html");
            return new Promise(() => {});
        }

        if (resposta.status === 204) return null;

        const texto = await resposta.text();
        const dados = texto ? JSON.parse(texto) : null;

        if (!resposta.ok) {
            // A API devolve { erro } nas regras de negócio e { errors } quando o JSON não bate com a classe
            const mensagem = dados?.erro ?? dados?.title ?? `Erro ${resposta.status}`;
            throw new ErroApi(mensagem, resposta.status, dados?.errors);
        }
        return dados;
    },

    listar(rota) {
        return this.requisitar("GET", rota);
    },

    inserir(rota, registro) {
        return this.requisitar("POST", rota, registro);
    },

    atualizar(rota, id, registro) {
        return this.requisitar("PUT", `${rota}/${id}`, registro);
    },

    excluir(rota, id) {
        return this.requisitar("DELETE", `${rota}/${id}`);
    }
};
