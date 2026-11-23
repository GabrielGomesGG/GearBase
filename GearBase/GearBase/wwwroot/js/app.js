const tela = {
    titulo: document.getElementById("titulo"),
    resumo: document.getElementById("resumo"),
    botaoNovo: document.getElementById("botao-novo"),
    filtro: document.getElementById("filtro"),
    busca: document.getElementById("busca"),
    aviso: document.getElementById("aviso"),
    areaTabela: document.getElementById("area-tabela"),
    areaInicio: document.getElementById("area-inicio"),
    dialogoForm: document.getElementById("dialogo-form"),
    form: document.getElementById("form"),
    formTitulo: document.getElementById("form-titulo"),
    formCampos: document.getElementById("form-campos"),
    formErro: document.getElementById("form-erro"),
    formSalvar: document.getElementById("form-salvar"),
    dialogoExcluir: document.getElementById("dialogo-excluir"),
    excluirTexto: document.getElementById("excluir-texto"),
    excluirErro: document.getElementById("excluir-erro"),
    excluirConfirmar: document.getElementById("excluir-confirmar")
};

const OPCOES_DE_LINHAS = [10, 20, 30];

const estado = {
    rota: null,
    entidade: null,
    registros: [],
    referencias: {},
    // Registros completos das outras telas, usados pelos filtros dos campos de referência
    listas: {},
    editando: null,
    excluindo: null,
    // Evita que a resposta de uma tela antiga apareça depois de o usuário já ter trocado de tela
    carga: 0,
    // Página de cada tela, para o usuário voltar onde estava ao trocar de tela e retornar
    paginas: {},
    linhasPorPagina: lerPreferencia("linhasPorPagina", 10),
    destacar: null,
    // Funcionário que entrou no sistema (vem de GET /api/login)
    usuario: null,
    abrirNovoAoCarregar: false
};

function lerPreferencia(chave, padrao) {
    try {
        const valor = Number(localStorage.getItem(chave));
        return OPCOES_DE_LINHAS.includes(valor) ? valor : padrao;
    } catch {
        return padrao;
    }
}

const moeda = new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" });

const SITUACOES = {
    ok: ["Vendido", "Concluída", "Concluído", "Realizado", "Ativo", "Entrada"],
    atencao: ["Reservado", "Em Manutenção", "Pendente", "Em Andamento", "Aguardando Peça", "Férias", "Afastado"],
    erro: ["Cancelada", "Cancelado", "Desligado", "Saída"]
};

const REGRAS_DO_BANCO = {
    chk_veiculos_ano: "O ano do modelo não pode ser menor que o ano de fabricação.",
    chk_td_km: "O km final não pode ser menor que o km inicial.",
    chk_td_avaliacao: "A avaliação deve ser de 1 a 5.",
    chk_vendas_valores: "A venda precisa ter pelo menos 1 parcela."
};

function maiuscula(texto) {
    return texto.charAt(0).toUpperCase() + texto.slice(1);
}

function genero(entidade, masculino, feminino) {
    return entidade.feminino ? feminino : masculino;
}

function criar(tag, classe, texto) {
    const elemento = document.createElement(tag);
    if (classe) elemento.className = classe;
    if (texto !== undefined) elemento.textContent = texto;
    return elemento;
}

function campoDe(entidade, nome) {
    return entidade.campos.find(c => c.nome === nome);
}

function campoDaColuna(entidade, coluna) {
    const nome = coluna.replace(/_(\w)/g, (_, letra) => letra.toUpperCase());
    return campoDe(entidade, nome);
}

function idDe(entidade, registro) {
    if (entidade.chaveComposta) return entidade.chaveComposta.map(c => registro[c]).join("/");
    return registro[entidade.id];
}

function nomeDe(nomeCampo, valor) {
    const campo = campoDe(estado.entidade, nomeCampo);
    return estado.referencias[campo.rota]?.get(valor) ?? `nº ${valor}`;
}

function descrever(registro) {
    return estado.entidade.descrever(registro, nomeDe);
}

/* Datas chegam da API como texto ("2024-06-01T10:00:00"). Cortar o texto evita
   que o navegador converta o horário para outro fuso. */
function formatar(campo, valor) {
    if ((valor === null || valor === undefined) && campo.vazio) return campo.vazio;
    if (valor === null || valor === undefined || valor === "") return "";

    switch (campo.tipo) {
        case "dinheiro":
            return moeda.format(valor);
        case "percentual":
            return `${Number(valor).toLocaleString("pt-BR", { minimumFractionDigits: 2, maximumFractionDigits: 2 })}%`;
        case "inteiro":
            return campo.sufixo ? Number(valor).toLocaleString("pt-BR") + campo.sufixo : String(valor);
        case "data": {
            const [ano, mes, dia] = valor.slice(0, 10).split("-");
            return `${dia}/${mes}/${ano}`;
        }
        case "dataHora": {
            const [data, hora] = valor.split("T");
            const [ano, mes, dia] = data.split("-");
            return `${dia}/${mes}/${ano} ${hora.slice(0, 5)}`;
        }
        case "booleano":
            return valor ? "Sim" : "Não";
        case "referencia":
            return nomeDe(campo.nome, valor);
        default:
            return String(valor);
    }
}

function ehNumero(campo) {
    return ["dinheiro", "percentual", "inteiro"].includes(campo.tipo);
}

function classeDaSituacao(valor) {
    for (const [tipo, valores] of Object.entries(SITUACOES)) {
        if (valores.includes(valor)) return `situacao situacao-${tipo}`;
    }
    return null;
}

function semAcento(texto) {
    return texto.normalize("NFD").replace(/[̀-ͯ]/g, "").toLowerCase();
}

function mostrarAviso(texto) {
    tela.aviso.textContent = texto;
    tela.aviso.hidden = false;
}

function esconderAviso() {
    tela.aviso.hidden = true;
}

function rotaAtual() {
    const rota = location.hash.replace(/^#\/?/, "");
    return entidades[rota] ? rota : "inicio";
}

function marcarMenu(rota) {
    const alvo = rota === "inicio" ? "#/" : `#/${rota}`;
    for (const link of document.querySelectorAll(".menu a")) {
        if (link.getAttribute("href") === alvo) link.setAttribute("aria-current", "page");
        else link.removeAttribute("aria-current");
    }
}

function mostrarErroDeCarga(area, texto, erro) {
    const caixa = criar("div", "estado estado-erro");
    caixa.append(criar("p", null, `${texto} ${erro.message}`));
    const botao = criar("button", "botao", "Tentar de novo");
    botao.type = "button";
    botao.addEventListener("click", abrirTela);
    caixa.append(botao);
    area.replaceChildren(caixa);
}

async function carregarReferencias(entidade) {
    const referenciadas = entidade.campos.filter(c => c.tipo === "referencia");
    const rotas = [...new Set(referenciadas.flatMap(c => [c.rota, ...(c.usa ?? [])]))];
    const resultados = await Promise.all(rotas.map(rota => api.listar(rota)));
    const referencias = {};
    const listas = {};

    rotas.forEach((rota, i) => {
        const outra = entidades[rota];
        listas[rota] = resultados[i];
        referencias[rota] = new Map(resultados[i].map(r => [r[outra.id], outra.descrever(r)]));
    });
    return { referencias, listas };
}

async function abrirTela() {
    const rota = rotaAtual();
    const carga = ++estado.carga;

    estado.rota = rota;
    estado.registros = [];
    tela.resumo.textContent = "";
    tela.filtro.value = "";
    esconderAviso();
    marcarMenu(rota);

    const inicio = rota === "inicio";
    tela.areaInicio.hidden = !inicio;
    tela.areaTabela.hidden = inicio;
    tela.busca.hidden = inicio;
    tela.botaoNovo.hidden = inicio;

    if (inicio) {
        estado.entidade = null;
        await abrirInicio(carga);
        return;
    }

    const entidade = entidades[rota];
    estado.entidade = entidade;
    document.title = `${maiuscula(entidade.plural)} · GearBase`;
    tela.titulo.textContent = maiuscula(entidade.plural);
    tela.botaoNovo.textContent = `${genero(entidade, "Novo", "Nova")} ${entidade.singular}`;
    tela.areaTabela.replaceChildren(criar("p", "estado", `Carregando ${entidade.plural}…`));

    try {
        const [registros, { referencias, listas }] = await Promise.all([api.listar(rota), carregarReferencias(entidade)]);
        if (carga !== estado.carga) return;
        estado.registros = registros;
        estado.referencias = referencias;
        estado.listas = listas;
        desenharTabela();

        if (estado.abrirNovoAoCarregar) {
            estado.abrirNovoAoCarregar = false;
            abrirFormulario(null);
        }
    } catch (erro) {
        if (carga !== estado.carga) return;
        mostrarErroDeCarga(tela.areaTabela, `Não foi possível carregar ${entidade.plural}.`, erro);
    }
}

// Tela inicial: o que precisa de atenção na loja, montado com os dados das outras telas
async function abrirInicio(carga) {
    document.title = "Início · GearBase";
    tela.titulo.textContent = "Início";
    tela.areaInicio.replaceChildren(criar("p", "estado", "Carregando o resumo da loja…"));

    let dados;
    try {
        const rotas = ["ordens-servico", "pecas", "test-drives", "vendas", "veiculos", "clientes", "fornecedores"];
        dados = await Promise.all(rotas.map(rota => api.listar(rota)));
    } catch (erro) {
        if (carga !== estado.carga) return;
        mostrarErroDeCarga(tela.areaInicio, "Não foi possível carregar o resumo da loja.", erro);
        return;
    }
    if (carga !== estado.carga) return;

    const [ordens, pecas, testDrives, vendas, veiculos, clientes, fornecedores] = dados;
    const nomeCliente = new Map(clientes.map(c => [c.clienteId, c.nome]));
    const nomeVeiculo = new Map(veiculos.map(v => [v.veiculoId, entidades.veiculos.descrever(v)]));
    const nomeFornecedor = new Map(fornecedores.map(f => [f.fornecedorId, entidades.fornecedores.descrever(f)]));
    const porData = campo => (a, b) => a[campo].localeCompare(b[campo]);

    const abertas = ordens
        .filter(o => ["Aberto", "Em Andamento", "Aguardando Peça"].includes(o.status))
        .sort(porData("dataAbertura"));
    const pecasBaixas = pecas
        .filter(p => p.quantidadeEstoque < p.estoqueMinimo)
        .sort((a, b) => (a.quantidadeEstoque - a.estoqueMinimo) - (b.quantidadeEstoque - b.estoqueMinimo));
    const agendados = testDrives.filter(t => t.status === "Agendado").sort(porData("dataAgendamento"));
    const pendentes = vendas.filter(v => v.statusVenda === "Pendente").sort(porData("dataVenda"));
    const disponiveis = veiculos.filter(v => v.status === "Disponível").length;
    const reservados = veiculos.filter(v => v.status === "Reservado").length;

    const contar = (n, singular, plural, nenhum) => n === 0 ? `${nenhum} ${singular}` : `${n} ${n === 1 ? singular : plural}`;
    tela.resumo.textContent =
        maiuscula(`${contar(abertas.length, "ordem de serviço em aberto", "ordens de serviço em aberto", "nenhuma")}, `) +
        `${contar(pecasBaixas.length, "peça abaixo do estoque mínimo", "peças abaixo do estoque mínimo", "nenhuma")}, ` +
        `${contar(agendados.length, "test drive agendado", "test drives agendados", "nenhum")} e ` +
        `${contar(pendentes.length, "venda pendente", "vendas pendentes", "nenhuma")}. ` +
        `No pátio, ${contar(disponiveis, "veículo disponível", "veículos disponíveis", "nenhum")} e ` +
        `${contar(reservados, "reservado", "reservados", "nenhum")}.`;

    const atalhos = criar("section", "atalhos");
    atalhos.setAttribute("aria-labelledby", "atalhos-titulo");
    const tituloAtalhos = criar("h2", null, "Cadastrar");
    tituloAtalhos.id = "atalhos-titulo";
    atalhos.append(tituloAtalhos);
    for (const rota of ["vendas", "ordens-servico", "test-drives", "clientes"]) {
        const entidade = entidades[rota];
        const botao = criar("button", "botao", `${genero(entidade, "Novo", "Nova")} ${entidade.singular}`);
        botao.type = "button";
        botao.addEventListener("click", () => {
            estado.abrirNovoAoCarregar = true;
            location.hash = `#/${rota}`;
        });
        atalhos.append(botao);
    }

    const situacao = valor => {
        const classe = classeDaSituacao(valor);
        return classe ? criar("span", classe, valor) : valor;
    };
    const dataHora = valor => formatar({ tipo: "dataHora" }, valor);

    const paineis = criar("div", "paineis");
    paineis.append(
        painel({
            titulo: "Ordens de serviço em aberto", linhas: abertas, rota: "ordens-servico",
            vazio: "Nenhuma ordem de serviço em aberto.",
            colunas: [
                { rotulo: "Nº", valor: o => String(o.ordemServicoId), numero: true },
                { rotulo: "Abertura", valor: o => formatar({ tipo: "data" }, o.dataAbertura) },
                { rotulo: "Cliente", valor: o => nomeCliente.get(o.clienteId) },
                { rotulo: "Veículo", valor: o => nomeVeiculo.get(o.veiculoId) },
                { rotulo: "Situação", valor: o => situacao(o.status) }
            ]
        }),
        painel({
            titulo: "Peças abaixo do estoque mínimo", linhas: pecasBaixas, rota: "pecas",
            vazio: "Todas as peças estão acima do estoque mínimo.",
            colunas: [
                { rotulo: "Peça", valor: p => p.nome },
                { rotulo: "Fornecedor", valor: p => nomeFornecedor.get(p.fornecedorId) },
                { rotulo: "Em estoque", valor: p => String(p.quantidadeEstoque), numero: true },
                { rotulo: "Mínimo", valor: p => String(p.estoqueMinimo), numero: true }
            ]
        }),
        painel({
            titulo: "Test drives agendados", linhas: agendados, rota: "test-drives",
            vazio: "Nenhum test drive agendado.",
            colunas: [
                { rotulo: "Data e hora", valor: t => dataHora(t.dataAgendamento) },
                { rotulo: "Cliente", valor: t => nomeCliente.get(t.clienteId) },
                { rotulo: "Veículo", valor: t => nomeVeiculo.get(t.veiculoId) }
            ]
        }),
        painel({
            titulo: "Vendas pendentes", linhas: pendentes, rota: "vendas",
            vazio: "Nenhuma venda pendente.",
            colunas: [
                { rotulo: "Nº", valor: v => String(v.vendaId), numero: true },
                { rotulo: "Data", valor: v => dataHora(v.dataVenda) },
                { rotulo: "Cliente", valor: v => nomeCliente.get(v.clienteId) },
                { rotulo: "Total", valor: v => moeda.format(v.valorTotal), numero: true }
            ]
        })
    );

    const hora = new Date().toLocaleTimeString("pt-BR", { hour: "2-digit", minute: "2-digit" });
    tela.areaInicio.replaceChildren(atalhos, paineis, criar("p", "nota", `Dados carregados às ${hora}.`));
}

const LINHAS_NO_PAINEL = 5;

function painel({ titulo, linhas, colunas, vazio, rota }) {
    const secao = criar("section", "painel");
    const h2 = criar("h2", null, titulo);
    h2.id = `painel-${rota}`;
    secao.setAttribute("aria-labelledby", h2.id);
    secao.append(h2);

    if (linhas.length === 0) {
        secao.append(criar("p", "painel-vazio", vazio));
    } else {
        const tabela = criar("table");
        tabela.setAttribute("aria-labelledby", h2.id);
        const cabecalho = criar("tr");
        for (const coluna of colunas) {
            const th = criar("th", coluna.numero ? "numero" : null, coluna.rotulo);
            th.scope = "col";
            cabecalho.append(th);
        }
        const thead = criar("thead");
        thead.append(cabecalho);
        const corpo = criar("tbody");
        for (const linha of linhas.slice(0, LINHAS_NO_PAINEL)) {
            const tr = criar("tr");
            for (const coluna of colunas) {
                const td = criar("td", coluna.numero ? "numero" : null);
                td.append(coluna.valor(linha) ?? "");
                tr.append(td);
            }
            corpo.append(tr);
        }
        tabela.append(thead, corpo);
        const rolagem = criar("div", "tabela-rolagem");
        rolagem.append(tabela);
        secao.append(rolagem);
    }

    const rodape = criar("div", "painel-rodape");
    rodape.append(criar("span", null, linhas.length > LINHAS_NO_PAINEL
        ? `Mostrando ${LINHAS_NO_PAINEL} de ${linhas.length}.`
        : ""));
    const link = criar("a", null, `Abrir ${entidades[rota].plural}`);
    link.href = `#/${rota}`;
    rodape.append(link);
    secao.append(rodape);
    return secao;
}

async function recarregarRegistros() {
    estado.registros = await api.listar(estado.rota);
    desenharTabela();
}

/* Registros em aberto primeiro; os encerrados (concluídos, cancelados, vendidos...) vão para o fim.
   O sort do JavaScript é estável, então dentro de cada grupo a ordem da API se mantém. */
function ordenarPorSituacao(entidade, registros) {
    const ordenados = entidade.ordenar ? [...registros].sort(entidade.ordenar) : registros;
    const campo = entidade.campos.find(c => c.encerrados);
    if (!campo) return ordenados;
    const encerrado = r => campo.encerrados.includes(r[campo.nome]) ? 1 : 0;
    return [...ordenados].sort((a, b) => encerrado(a) - encerrado(b));
}

const LARGURA_MAXIMA = 40;

/* Largura de cada coluna pelo maior texto entre todos os registros, inclusive os das outras páginas:
   assim as colunas não mudam de tamanho quando o usuário troca de página */
function larguraDasColunas(colunas, linhas) {
    return colunas.map((coluna, i) => {
        let maior = coluna.rotulo.length;
        for (const { registro, textos } of linhas) {
            const alerta = coluna.alerta?.(registro);
            maior = Math.max(maior, textos[i].length + (alerta ? alerta.length + 2 : 0));
        }
        const folga = coluna.situacao ? 3 : 1;
        return `calc(${Math.min(maior, LARGURA_MAXIMA) + folga}ch + 24px)`;
    });
}

function desenharTabela() {
    const entidade = estado.entidade;
    const colunas = entidade.colunas.map(nome => campoDe(entidade, nome));
    const total = estado.registros.length;
    const termo = semAcento(tela.filtro.value.trim());

    const linhas = ordenarPorSituacao(entidade, estado.registros).map(registro => ({
        registro,
        textos: colunas.map(c => c.exibir ? c.exibir(registro) : formatar(c, registro[c.nome]))
    }));
    const visiveis = termo ? linhas.filter(l => semAcento(l.textos.join(" ")).includes(termo)) : linhas;

    tela.resumo.textContent = termo
        ? `Mostrando ${visiveis.length} de ${total} ${entidade.plural}.`
        : `${total} ${total === 1 ? entidade.singular : entidade.plural}.`;
    if (entidade.resumir && visiveis.length > 0) {
        tela.resumo.textContent += ` ${entidade.resumir(visiveis.map(l => l.registro))}`;
    }

    if (total === 0) {
        const novo = `${genero(entidade, "Novo", "Nova")} ${entidade.singular}`;
        tela.areaTabela.replaceChildren(criar("p", "estado",
            `${genero(entidade, "Nenhum", "Nenhuma")} ${entidade.singular} ${genero(entidade, "cadastrado", "cadastrada")}. ` +
            `Clique em "${novo}" para cadastrar ${genero(entidade, "o primeiro", "a primeira")}.`));
        return;
    }

    if (visiveis.length === 0) {
        const caixa = criar("div", "estado");
        caixa.append(criar("p", null, `Nada encontrado para "${tela.filtro.value.trim()}".`));
        const limpar = criar("button", "botao", "Limpar busca");
        limpar.type = "button";
        limpar.addEventListener("click", () => {
            tela.filtro.value = "";
            desenharTabela();
            tela.filtro.focus();
        });
        caixa.append(limpar);
        tela.areaTabela.replaceChildren(caixa);
        return;
    }

    const porPagina = estado.linhasPorPagina;
    const totalPaginas = Math.ceil(visiveis.length / porPagina);

    // Depois de cadastrar, vai para a página onde o registro novo ficou
    if (estado.destacar !== null) {
        const posicao = visiveis.findIndex(l => String(idDe(entidade, l.registro)) === String(estado.destacar));
        if (posicao >= 0) estado.paginas[estado.rota] = Math.floor(posicao / porPagina) + 1;
    }
    const pagina = Math.min(Math.max(estado.paginas[estado.rota] ?? 1, 1), totalPaginas);
    estado.paginas[estado.rota] = pagina;
    const daPagina = visiveis.slice((pagina - 1) * porPagina, pagina * porPagina);

    const tabela = criar("table", "tabela-fixa");
    tabela.setAttribute("aria-labelledby", "titulo");

    const grupo = criar("colgroup");
    for (const largura of [...larguraDasColunas(colunas, linhas), "calc(13ch + 56px)"]) {
        const col = criar("col");
        col.style.width = largura;
        grupo.append(col);
    }
    tabela.append(grupo);

    const cabecalho = criar("tr");
    for (const coluna of colunas) {
        const th = criar("th", ehNumero(coluna) ? "numero" : null, coluna.rotulo);
        th.scope = "col";
        cabecalho.append(th);
    }
    const thAcoes = criar("th", "coluna-acoes", "Ações");
    thAcoes.scope = "col";
    cabecalho.append(thAcoes);
    const thead = criar("thead");
    thead.append(cabecalho);
    tabela.append(thead);

    const corpo = criar("tbody");
    for (const { registro, textos } of daPagina) {
        const tr = criar("tr");
        if (estado.destacar !== null && String(idDe(entidade, registro)) === String(estado.destacar)) {
            tr.className = "linha-nova";
        }

        colunas.forEach((coluna, i) => {
            const td = criar("td", ehNumero(coluna) ? "numero" : null);
            const classe = coluna.situacao ? classeDaSituacao(textos[i]) : null;
            if (classe) td.append(criar("span", classe, textos[i]));
            else td.textContent = textos[i];
            // Texto maior que a coluna aparece cortado com "…"; o texto inteiro fica na dica do mouse
            if (textos[i].length > LARGURA_MAXIMA) td.title = textos[i];

            const alerta = coluna.alerta?.(registro);
            if (alerta) td.append(criar("span", "alerta", alerta));
            tr.append(td);
        });

        const nome = descrever(registro);
        const tdAcoes = criar("td", "coluna-acoes");
        const automatico = entidade.automatico?.(registro);
        if (automatico) {
            tdAcoes.append(criar("span", "nota-linha", automatico));
            tr.append(tdAcoes);
            corpo.append(tr);
            continue;
        }
        const editar = criar("button", "botao-linha", "Editar");
        editar.type = "button";
        editar.setAttribute("aria-label", `Editar ${entidade.singular} ${nome}`);
        editar.addEventListener("click", () => abrirFormulario(registro));
        const excluir = criar("button", "botao-linha botao-linha-perigo", "Excluir");
        excluir.type = "button";
        excluir.setAttribute("aria-label", `Excluir ${entidade.singular} ${nome}`);
        excluir.addEventListener("click", () => abrirExclusao(registro));
        tdAcoes.append(editar, excluir);
        tr.append(tdAcoes);

        corpo.append(tr);
    }

    // Completa a última página com linhas vazias, para a paginação não subir na tela
    if (totalPaginas > 1) {
        for (let i = daPagina.length; i < porPagina; i++) {
            const vazia = criar("tr", "linha-vazia");
            vazia.setAttribute("aria-hidden", "true");
            const td = criar("td");
            td.colSpan = colunas.length + 1;
            vazia.append(td);
            corpo.append(vazia);
        }
    }
    tabela.append(corpo);
    estado.destacar = null;

    const rolagem = criar("div", "tabela-rolagem");
    rolagem.append(tabela);
    const partes = [rolagem];
    if (visiveis.length > OPCOES_DE_LINHAS[0]) partes.push(desenharPaginacao(pagina, totalPaginas, visiveis.length));
    tela.areaTabela.replaceChildren(...partes);
}

function irParaPagina(pagina) {
    estado.paginas[estado.rota] = pagina;
    desenharTabela();
    // Mantém o foco do teclado na paginação, que acabou de ser redesenhada
    tela.areaTabela.querySelector('.pagina[aria-current="page"]')?.focus();
}

// Até 7 páginas mostra todas; acima disso, a primeira, a última e as vizinhas da atual
function numerosDasPaginas(atual, total) {
    if (total <= 7) return Array.from({ length: total }, (_, i) => i + 1);
    const perto = [1, atual - 1, atual, atual + 1, total].filter(n => n >= 1 && n <= total);
    const numeros = [...new Set(perto)].sort((a, b) => a - b);
    const comLacunas = [];
    numeros.forEach((n, i) => {
        if (i > 0 && n - numeros[i - 1] > 1) comLacunas.push(null);
        comLacunas.push(n);
    });
    return comLacunas;
}

function desenharPaginacao(pagina, totalPaginas, totalLinhas) {
    const porPagina = estado.linhasPorPagina;
    const rodape = criar("div", "paginacao");

    const primeira = (pagina - 1) * porPagina + 1;
    const ultima = Math.min(pagina * porPagina, totalLinhas);
    rodape.append(criar("p", null, `${primeira} a ${ultima} de ${totalLinhas}`));

    const linhas = criar("div", "paginacao-linhas");
    const rotulo = criar("label", null, "Linhas por página");
    rotulo.htmlFor = "linhas-por-pagina";
    const seletor = criar("select");
    seletor.id = "linhas-por-pagina";
    for (const opcao of OPCOES_DE_LINHAS) seletor.append(new Option(String(opcao), String(opcao)));
    seletor.value = String(porPagina);
    seletor.addEventListener("change", () => {
        estado.linhasPorPagina = Number(seletor.value);
        try {
            localStorage.setItem("linhasPorPagina", seletor.value);
        } catch {
            // Sem armazenamento, a escolha vale só nesta aba
        }
        estado.paginas[estado.rota] = 1;
        desenharTabela();
        document.getElementById("linhas-por-pagina")?.focus();
    });
    linhas.append(rotulo, seletor);
    rodape.append(linhas);

    const nav = criar("nav", "paginas");
    nav.setAttribute("aria-label", "Páginas da tabela");

    const botaoPagina = (texto, destino, rotuloAcessivel) => {
        const botao = criar("button", "pagina", texto);
        botao.type = "button";
        if (rotuloAcessivel) botao.setAttribute("aria-label", rotuloAcessivel);
        if (destino === pagina) botao.setAttribute("aria-current", "page");
        else if (destino < 1 || destino > totalPaginas) botao.disabled = true;
        else botao.addEventListener("click", () => irParaPagina(destino));
        return botao;
    };

    nav.append(botaoPagina("Anterior", pagina - 1));
    for (const numero of numerosDasPaginas(pagina, totalPaginas)) {
        if (numero === null) nav.append(criar("span", "reticencias", "…"));
        else nav.append(botaoPagina(String(numero), numero, `Página ${numero}`));
    }
    nav.append(botaoPagina("Próxima", pagina + 1));
    rodape.append(nav);

    return rodape;
}

/* Dinheiro e percentual são digitados como numa maquininha de cartão: os números entram pela direita
   e a vírgula fica sempre antes dos dois últimos (digitar 123456 mostra R$ 1.234,56) */
const DIGITOS_MAXIMOS = { dinheiro: 10, percentual: 5 }; // DECIMAL(10,2) e DECIMAL(5,2) no banco

function ehDecimal(campo) {
    return campo.tipo === "dinheiro" || campo.tipo === "percentual";
}

function formatarDecimal(campo, numero) {
    if (numero === null || numero === undefined || numero === "") return "";
    return campo.tipo === "dinheiro"
        ? moeda.format(numero)
        : Number(numero).toLocaleString("pt-BR", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

function lerDecimal(texto) {
    const digitos = texto.replace(/\D/g, "");
    return digitos === "" ? null : Number(digitos) / 100;
}

function mascararDecimal(entrada, campo) {
    entrada.addEventListener("input", evento => {
        let digitos = entrada.value.replace(/\D/g, "").replace(/^0+/, "").slice(0, DIGITOS_MAXIMOS[campo.tipo]);
        // Digitar 0 no campo vazio mostra 0,00; apagar até zerar deixa o campo vazio
        if (digitos === "" && evento.inputType?.startsWith("insert") && /\d/.test(evento.data ?? "")) digitos = "0";
        entrada.value = digitos === "" ? "" : formatarDecimal(campo, Number(digitos) / 100);
    });
}

function agoraParaCampo() {
    const agora = new Date();
    agora.setMinutes(agora.getMinutes() - agora.getTimezoneOffset());
    return agora.toISOString().slice(0, 16);
}

/* Monta as opções de um select. Um campo com "filtro" só mostra o que a regra permite
   (só vendedores, só veículos da frota do cliente...), mas mantém o valor que o registro já tinha */
function preencherOpcoes(entrada, campo, valor, valores) {
    let opcoes;
    if (campo.tipo === "opcoes") {
        opcoes = campo.opcoes.map(o => [o, o]);
    } else {
        opcoes = [...(estado.referencias[campo.rota] ?? new Map())]
            .sort((a, b) => String(a[1]).localeCompare(String(b[1]), "pt-BR"));

        if (campo.filtro) {
            const id = entidades[campo.rota].id;
            const original = estado.editando?.[campo.nome];
            const mesmaOrigem = !campo.dependeDe || valores[campo.dependeDe] === estado.editando?.[campo.dependeDe];
            const permitidos = new Set(estado.listas[campo.rota]
                .filter(r => (mesmaOrigem && r[id] === original) || campo.filtro(r, valores, estado.listas))
                .map(r => r[id]));
            opcoes = opcoes.filter(([valorOpcao]) => permitidos.has(valorOpcao));
        }
    }

    let primeira = campo.obrigatorio ? "Selecione" : (campo.vazio ?? "Não informado");
    if (campo.dependeDe && !valores[campo.dependeDe]) primeira = campo.textoSemOrigem;
    else if (opcoes.length === 0 && campo.textoSemOpcoes) primeira = campo.textoSemOpcoes;

    entrada.replaceChildren(new Option(primeira, ""), ...opcoes.map(([valorOpcao, texto]) => new Option(texto, valorOpcao)));
    entrada.value = valor ?? "";
    if (entrada.selectedIndex < 0) entrada.value = "";
}

function criarEntrada(campo, valor, bloqueado, valores) {
    let entrada;

    if (campo.tipo === "opcoes" || campo.tipo === "referencia") {
        entrada = criar("select");
        preencherOpcoes(entrada, campo, valor, valores);
    } else if (campo.tipo === "textoLongo") {
        entrada = criar("textarea");
        entrada.value = valor ?? "";
    } else {
        entrada = criar("input");
        const tipos = {
            email: "email", url: "url", inteiro: "number",
            data: "date", dataHora: "datetime-local", booleano: "checkbox"
        };
        entrada.type = tipos[campo.tipo] ?? "text";

        if (campo.tipo === "inteiro") entrada.step = "1";
        if (ehDecimal(campo)) {
            entrada.inputMode = "numeric";
            entrada.placeholder = formatarDecimal(campo, 0);
            mascararDecimal(entrada, campo);
        }
        if (campo.min !== undefined) {
            if (entrada.type === "number") entrada.min = campo.min;
            else entrada.minLength = campo.min;
        }
        if (campo.maxValor !== undefined) entrada.max = campo.maxValor;
        if (campo.max !== undefined) entrada.maxLength = campo.max;

        if (campo.tipo === "booleano") entrada.checked = Boolean(valor);
        else if (campo.tipo === "dataHora") entrada.value = valor ? valor.slice(0, 16) : "";
        else if (campo.tipo === "data") entrada.value = valor ? valor.slice(0, 10) : "";
        else if (ehDecimal(campo)) entrada.value = formatarDecimal(campo, valor);
        else entrada.value = valor ?? "";
    }

    entrada.id = `campo-${campo.nome}`;
    entrada.name = campo.nome;
    entrada.required = Boolean(campo.obrigatorio) && campo.tipo !== "booleano";
    entrada.disabled = bloqueado;
    return entrada;
}

function abrirFormulario(registro) {
    const entidade = estado.entidade;
    const novo = !registro;
    estado.editando = registro ?? null;
    esconderAviso();

    tela.formTitulo.textContent = novo
        ? `${genero(entidade, "Novo", "Nova")} ${entidade.singular}`
        : `Editar ${entidade.singular} ${descrever(registro)}`;
    tela.formErro.hidden = true;
    tela.formSalvar.disabled = false;
    tela.formSalvar.textContent = novo ? `Cadastrar ${entidade.singular}` : "Salvar alterações";

    // Valores que os filtros dos selects olham (ex.: o cliente, para mostrar só os veículos dele)
    const valores = novo ? Object.fromEntries(entidade.campos.map(c => [c.nome, c.padrao])) : registro;
    const campos = [];
    for (const campo of entidade.campos.filter(c => !c.calculado)) {
        let valor = novo ? campo.padrao : registro[campo.nome];
        // Num cadastro novo, o funcionário já vem preenchido com quem está logado (se o cargo permitir)
        if (novo && campo.rota === "funcionarios") valor = estado.usuario?.funcionarioId;
        if (novo && campo.tipo === "dataHora" && campo.obrigatorio) valor = agoraParaCampo();
        // Na chave composta, trocar veículo ou opcional seria outro registro: só dá para mudar os outros campos
        const bloqueado = !novo && Boolean(entidade.chaveComposta?.includes(campo.nome));
        const entrada = criarEntrada(campo, valor, bloqueado, valores);

        const caixa = criar("div", "campo");
        if (campo.tipo === "booleano") {
            caixa.classList.add("campo-marcar");
            const rotulo = criar("label");
            rotulo.htmlFor = entrada.id;
            rotulo.append(entrada, document.createTextNode(campo.rotulo));
            caixa.append(rotulo);
        } else {
            if (campo.tipo === "textoLongo") caixa.classList.add("campo-largo");
            const rotulo = criar("label", null, campo.rotulo);
            rotulo.htmlFor = entrada.id;
            if (!campo.obrigatorio) rotulo.append(criar("span", "opcional", " (opcional)"));
            caixa.append(rotulo, entrada);
        }
        campos.push(caixa);
    }
    tela.formCampos.replaceChildren(...campos);

    for (const campo of entidade.campos.filter(c => c.dependeDe)) {
        const origem = tela.form.elements[campo.dependeDe];
        const entrada = tela.form.elements[campo.nome];
        origem.addEventListener("change", () => {
            preencherOpcoes(entrada, campo, Number(entrada.value) || null, { [campo.dependeDe]: Number(origem.value) || null });
        });
    }

    tela.dialogoForm.showModal();
    tela.formCampos.querySelector("input:not(:disabled), select:not(:disabled), textarea")?.focus();
}

function lerFormulario() {
    const registro = { ...(estado.editando ?? {}) };

    for (const campo of estado.entidade.campos.filter(c => !c.calculado)) {
        const entrada = tela.form.elements[campo.nome];
        const texto = entrada.value.trim();

        if (campo.tipo === "booleano") registro[campo.nome] = entrada.checked;
        else if (texto === "") registro[campo.nome] = null;
        else if (ehDecimal(campo)) registro[campo.nome] = lerDecimal(texto);
        else if (ehNumero(campo) || campo.tipo === "referencia") registro[campo.nome] = Number(texto);
        else if (campo.tipo === "dataHora") registro[campo.nome] = `${texto}:00`;
        else registro[campo.nome] = texto;
    }
    return registro;
}

function traduzirErro(erro) {
    const entidade = estado.entidade;
    const mensagem = erro.message;
    let partes;

    if (erro.detalhes) {
        const chave = Object.keys(erro.detalhes)[0] ?? "";
        const campo = entidade.campos.find(c => c.nome.toLowerCase() === chave.replace(/^\$\./, "").toLowerCase());
        return campo ? `Confira o campo ${campo.rotulo}.` : "Confira os campos do formulário.";
    }

    if ((partes = mensagem.match(/Duplicate entry '(.*)' for key '\w+\.(\w+)'/))) {
        const campo = campoDaColuna(entidade, partes[2]);
        if (!campo) return `${genero(entidade, "Este", "Esta")} ${entidade.singular} já está ${genero(entidade, "cadastrado", "cadastrada")}.`;
        const valor = campo.tipo === "referencia" ? nomeDe(campo.nome, Number(partes[1])) : partes[1];
        return `Já existe ${genero(entidade, "um", "uma")} ${entidade.singular} com ${campo.rotulo.toLowerCase()} ${valor}.`;
    }

    if ((partes = mensagem.match(/Cannot delete or update a parent row: .*?`\w+`\.`(\w+)`/))) {
        const outra = entidades[partes[1].replace(/_/g, "-")];
        const onde = outra ? `"${maiuscula(outra.plural)}"` : "outra tela";
        return `Não é possível excluir. Há registros em ${onde} que usam ${genero(entidade, "este", "esta")} ${entidade.singular}.`;
    }

    if ((partes = mensagem.match(/Cannot add or update a child row: .*FOREIGN KEY \(`(\w+)`\)/))) {
        const campo = campoDaColuna(entidade, partes[1]);
        return `O item escolhido em ${campo ? campo.rotulo : "um dos campos"} não existe mais. Atualize a tela e escolha de novo.`;
    }

    if ((partes = mensagem.match(/Check constraint '(\w+)' is violated/))) {
        return REGRAS_DO_BANCO[partes[1]] ?? "Algum valor está fora do permitido. Confira os números do formulário.";
    }

    if ((partes = mensagem.match(/Data too long for column '(\w+)'/))) {
        const campo = campoDaColuna(entidade, partes[1]);
        return `O campo ${campo ? campo.rotulo : partes[1]} passou do tamanho permitido.`;
    }

    if (/Data truncated|Incorrect \w+ value/.test(mensagem)) {
        return "Algum valor não é aceito pelo banco. Confira as opções escolhidas.";
    }

    return mensagem;
}

// Janela que pede matrícula e senha de um gerente. Devolve { matricula, senha }, ou null se cancelar.
function pedirLiberacao(texto) {
    const dialogo = document.getElementById("dialogo-liberacao");
    const form = document.getElementById("form-liberacao");
    document.getElementById("liberacao-texto").textContent = texto;
    form.reset();

    return new Promise(resolve => {
        const terminar = resposta => {
            form.onsubmit = null;
            dialogo.onclose = null;
            dialogo.close();
            resolve(resposta);
        };
        form.onsubmit = evento => {
            evento.preventDefault();
            terminar({ matricula: form.elements.matricula.value.trim(), senha: form.elements.senha.value });
        };
        dialogo.onclose = () => terminar(null);
        document.getElementById("liberacao-cancelar").onclick = () => terminar(null);
        dialogo.showModal();
        form.elements.matricula.focus();
    });
}

// Campo com limite (ex.: parcelas acima de 60) que passou do limite e precisa de liberação do gerente
function precisaDeLiberacao(entidade, registro, novo) {
    const campo = entidade.campos.find(c => c.limiteSemLiberacao);
    if (!campo || estado.usuario?.podeLiberar) return null;
    const valor = registro[campo.nome];
    if (valor === null || valor <= campo.limiteSemLiberacao) return null;
    // Editar sem mudar o valor não pede de novo (já foi liberado quando cadastraram)
    if (!novo && valor === estado.editando[campo.nome]) return null;
    return campo;
}

async function salvar(evento) {
    evento.preventDefault();
    const entidade = estado.entidade;
    const novo = !estado.editando;
    const registro = lerFormulario();

    const limitado = precisaDeLiberacao(entidade, registro, novo);
    if (limitado) {
        registro.liberacao = await pedirLiberacao(
            `Acima de ${limitado.limiteSemLiberacao} ${limitado.rotulo.toLowerCase()}, é preciso a liberação de um gerente. ` +
            "Peça para ele digitar a matrícula e a senha.");
        if (!registro.liberacao) return;
    }

    tela.formErro.hidden = true;
    tela.formSalvar.disabled = true;
    tela.formSalvar.textContent = "Salvando…";

    try {
        let salvo = registro;
        if (novo) salvo = await api.inserir(estado.rota, registro);
        else await api.atualizar(estado.rota, idDe(entidade, estado.editando), registro);

        tela.dialogoForm.close();
        estado.destacar = idDe(entidade, salvo);
        await recarregarRegistros();
        const acao = novo ? genero(entidade, "cadastrado", "cadastrada") : genero(entidade, "atualizado", "atualizada");
        mostrarAviso(`${maiuscula(entidade.singular)} ${descrever(salvo)} ${acao}.`);
    } catch (erro) {
        tela.formErro.textContent = traduzirErro(erro);
        tela.formErro.hidden = false;
        tela.formSalvar.disabled = false;
        tela.formSalvar.textContent = novo ? `Cadastrar ${entidade.singular}` : "Salvar alterações";
    }
}

function abrirExclusao(registro) {
    const entidade = estado.entidade;
    estado.excluindo = registro;
    esconderAviso();
    tela.excluirTexto.textContent =
        `Excluir ${genero(entidade, "o", "a")} ${entidade.singular} ${descrever(registro)}? Essa ação não pode ser desfeita.`;
    tela.excluirErro.hidden = true;
    tela.excluirConfirmar.disabled = false;
    tela.dialogoExcluir.showModal();
}

async function confirmarExclusao() {
    const entidade = estado.entidade;
    const registro = estado.excluindo;
    const nome = descrever(registro);

    tela.excluirErro.hidden = true;
    tela.excluirConfirmar.disabled = true;

    try {
        await api.excluir(estado.rota, idDe(entidade, registro));
        tela.dialogoExcluir.close();
        await recarregarRegistros();
        mostrarAviso(`${maiuscula(entidade.singular)} ${nome} ${genero(entidade, "excluído", "excluída")}.`);
    } catch (erro) {
        tela.excluirErro.textContent = traduzirErro(erro);
        tela.excluirErro.hidden = false;
        tela.excluirConfirmar.disabled = false;
    }
}

tela.botaoNovo.addEventListener("click", () => abrirFormulario(null));
tela.filtro.addEventListener("input", () => {
    estado.paginas[estado.rota] = 1;
    desenharTabela();
});
tela.form.addEventListener("submit", salvar);
document.getElementById("form-cancelar").addEventListener("click", () => tela.dialogoForm.close());
document.getElementById("excluir-cancelar").addEventListener("click", () => tela.dialogoExcluir.close());
tela.excluirConfirmar.addEventListener("click", confirmarExclusao);
window.addEventListener("hashchange", abrirTela);

document.getElementById("sair").addEventListener("click", async () => {
    await api.requisitar("DELETE", "login");
    location.replace("login.html");
});

async function iniciar() {
    estado.usuario = await api.requisitar("GET", "login");
    document.getElementById("usuario-nome").textContent = `${estado.usuario.nome} · ${estado.usuario.cargo}`;
    abrirTela();
}

iniciar().catch(erro => mostrarAviso(`Não foi possível abrir o sistema. ${erro.message}`));
