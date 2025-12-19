# MPC Animal Simulation
Uma comparação entre a tomada de decisões de Máquina de Estado Finita e Modelo Preditivo de Controle em simulações controladas. O projeto contém cenas e prefabs para testar interações entre agentes e recursos (comida), além de componentes para monitoração da simulação.

## Visão geral
- Agente com máquina de estados finita (FSM)
- Agente com modelo de controle preditivo (MPC)
- Movimentação, percepção e combate básicos
- Prefabs: `Enemy`, `Food`, `SimulationCommon`, `SimWatcher`
- Cenas de exemplo em `Assets/Scenes` (`MPC.unity`, `FSM.unity`)

## Versão da Unity
6000.0.56f1

## Como rodar
1. Abra o Unity Hub e adicione o projeto (pasta raiz do repositório).
2. Abra a cena desejada em `Assets/Scenes` (por exemplo `MPC.unity`).
3. Clique em Play no Editor para iniciar a simulação.

## Estrutura do repositório
- `Assets/Prefabs` — prefabs reutilizáveis (Enemy, Food, SimulationCommon, SimWatcher)
- `Assets/Scripts` — scripts principais: `FSM.cs`, `AIMovement.cs`, `AIHead.cs`, `AIStatus.cs`, `AICombat.cs`, `SimManager.cs`, `GameManager.cs`, `MPC.cs`
- `Assets/Scenes` — cenas de exemplo
- `SimResults` — Resultados da simulação
- `Packages/manifest.json` — dependências do projeto

## Uso rápido / Controles
- Ajuste parâmetros da simulação selecionando `SimulationCommon` ou `SimManager` no Inspector (quantidade de inimigos, spawn de comida, etc.).
- Inicie com Play no Editor.
- Use o `SimWatcher` para visualizar estatísticas básicas.

## Licença
Licença do projeto: [LICENSE](https://github.com/apecuca/MPC-Animal-Simulation/blob/main/ATTRIBUTION.md)

## Referências
- Unity Documentation: https://docs.unity3d.com/
- Pacotes usados (ver `Packages/manifest.json`)
