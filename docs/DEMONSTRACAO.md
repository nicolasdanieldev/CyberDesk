# 🛡️ CyberDesk — Demonstração técnica

Este roteiro registra um fluxo funcional para demonstrar os cinco formulários do projeto acadêmico de Linguagem de Programação 2, com foco em **C# e Programação Orientada a Objetos**.

## 1. Dashboard (`frmPrincipal`)

Inicie a aplicação, atualize os indicadores e mostre a listagem de incidentes. Explique o uso de classes, métodos e listas para transformar registros do MySQL em informações para a interface.

## 2. Equipamentos (`frmAtivos`)

Cadastre dois equipamentos, após conferir que os IPs ainda estão livres:

| Nome | IP | Setor | Responsável |
|---|---|---|---|
| DEMO LP2 - Servidor TI | 10.77.26.10 | Infraestrutura | Equipe de Segurança |
| DEMO LP2 - Notebook RH | 10.77.26.20 | Recursos Humanos | Suporte de TI |

Demonstre dois tratamentos de erros distintos:
- Tente cadastrar um equipamento com IP `999.999.999.999`: a classe de domínio deve rejeitar o formato inválido.
- Tente cadastrar outro equipamento com IP `10.77.26.10`: a restrição `UNIQUE` do MySQL deve impedir a duplicidade.

## 3. Incidentes (`frmIncidentes`)

- Vincule ao servidor uma ocorrência de tentativas de login indevido, com impacto 4 e probabilidade 5: pontuação **20/25 — Crítico**.
- Vincule ao notebook uma ocorrência de falha de atualização, com impacto 3 e probabilidade 3: pontuação **9/25 — Moderado**.
- Explique a regra de cálculo no `CalculadoraRisco`, os objetos `Incidente` e o tratamento de entradas.

## 4. Histórico (`frmHistorico`)

Altere o status do incidente do servidor de **Aberto** para **Em análise** e informe o motivo. Consulte o registro e destaque a transação de banco que grava a mudança e seu histórico.

## 5. Relatórios (`frmRelatorios`)

Atualize indicadores e gráficos após os cadastros. Confira a classificação de risco e a distribuição por status. Exporte registros filtrados em CSV.

## 6. Verificação no MySQL Workbench

```sql
USE cyberdesk;

SELECT id, nome, endereco_ip, setor
FROM ativos
WHERE endereco_ip IN ('10.77.26.10', '10.77.26.20');

SELECT i.id, i.titulo, a.nome AS equipamento,
       i.impacto * i.probabilidade AS pontuacao_risco,
       i.status
FROM incidentes i
JOIN ativos a ON a.id = i.ativo_id
WHERE a.endereco_ip IN ('10.77.26.10', '10.77.26.20');

SELECT h.id, i.titulo, h.status_anterior, h.status_novo,
       h.observacao, h.data_alteracao
FROM historico_incidentes h
JOIN incidentes i ON i.id = h.incidente_id
ORDER BY h.data_alteracao DESC;
```

> As consultas são somente leitura: o cadastro e as atualizações devem acontecer pelo Windows Forms. Caso existam registros anteriores, os indicadores gerais poderão exibir totais maiores.
