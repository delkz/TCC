# Sistema de Build - Documentacao Tecnica

## 1. Objetivo

Este documento descreve o sistema de construcao e remocao de estruturas durante o gameplay, incluindo:
- Modelo de dados de construcoes.
- Fluxo de preview, selecao e colocacao.
- Fluxo de destruicao e reembolso.
- Integracoes com grid, economia, HUD e audio.
- API publica principal e pontos de extensao.

## 2. Visao geral da arquitetura

Arquivos centrais:
- [Assets/Scripts/Build/BuildPreviewController.cs](../Assets/Scripts/Build/BuildPreviewController.cs)
- [Assets/Scripts/Build/BuildMode.cs](../Assets/Scripts/Build/BuildMode.cs)
- [Assets/Scripts/Build/Buildable.cs](../Assets/Scripts/Build/Buildable.cs)
- [Assets/Scripts/Data/BuildingData.cs](../Assets/Scripts/Data/BuildingData.cs)
- [Assets/Scripts/Grid/GridManager.cs](../Assets/Scripts/Grid/GridManager.cs)
- [Assets/Scripts/Systems/GoldManager.cs](../Assets/Scripts/Systems/GoldManager.cs)
- [Assets/Scripts/UI/GameHUDController.cs](../Assets/Scripts/UI/GameHUDController.cs)
- [Assets/Scripts/Systems/Audio/GameAudioEvents.cs](../Assets/Scripts/Systems/Audio/GameAudioEvents.cs)
- [Assets/Scripts/Entities/DefenseTower.cs](../Assets/Scripts/Entities/DefenseTower.cs)

Responsabilidades por classe:
- BuildPreviewController: orquestra modo de build/destroy, preview em grid, input de usuario, colocacao e remocao.
- BuildMode: enum com os estados Build e Destroy.
- Buildable: adaptador de runtime que expoe dados de destruicao/reembolso para uma construcao instanciada.
- BuildingData: ScriptableObject com definicao de custo, prefab, regras e stats da estrutura.
- GridManager: valida celula buildavel, converte coordenadas e registra ocupantes em cada celula.
- GoldManager: valida saldo, gasta ouro e aplica reembolso.
- GameHUDController: atualiza texto de modo e construcao selecionada.
- GameAudioEvents: publica eventos de audio para build colocado/removido.
- DefenseTower: consome BuildingData em runtime para dano, range e cooldown.

## 3. Modelo de dados de construcao

### 3.1 Asset de BuildingData

[Assets/Scripts/Data/BuildingData.cs](../Assets/Scripts/Data/BuildingData.cs)
- Basic Info:
- buildingName: nome exibido na HUD.
- prefab: prefab instanciado na celula.
- icon: sprite opcional aplicado no SpriteRenderer apos instanciar.
- Economy:
- buildCost: custo de compra.
- refundPercent: percentual de retorno na destruicao (0..1).
- Rules:
- canBeDestroyed: controla se a estrutura pode ser vendida/removida.
- Tower Stats:
- damage, attackCooldown, range.
- Effects:
- slowAmount, slowDuration, knockback.

### 3.2 Componente Buildable

[Assets/Scripts/Build/Buildable.cs](../Assets/Scripts/Build/Buildable.cs)
- Data: referencia ao BuildingData usado na estrutura instanciada.
- CanBeDestroyed: proxy para data.canBeDestroyed.
- GetRefundValue: calcula reembolso com floor(buildCost * refundPercent).

## 4. Modo de operacao

[Assets/Scripts/Build/BuildMode.cs](../Assets/Scripts/Build/BuildMode.cs)
- Build: coloca estruturas.
- Destroy: remove estruturas existentes elegiveis.

Troca de modo:
- Tecla Tab alterna Build <-> Destroy.
- HUD e atualizada via GameHUDController.UpdateMode.

## 5. Pipeline de runtime

### 5.1 Inicializacao

[Assets/Scripts/Build/BuildPreviewController.cs](../Assets/Scripts/Build/BuildPreviewController.cs)
- Start:
1. Instancia previewPrefab.
2. Captura SpriteRenderer do preview.
3. Atualiza HUD com construcao selecionada e custo.

### 5.2 Loop principal

BuildPreviewController.Update executa:
1. HandleModeSwitch: alterna modo com Tab.
2. UpdatePreview: calcula celula atual e cor de validacao.
3. HandleInput: interpreta clique esquerdo e teclas de selecao.

### 5.3 Selecao de construcao

Em Build mode:
- Tecla Q: seleciona construcao anterior.
- Tecla E: seleciona proxima construcao.
- Selecao e circular (wrap-around no array).
- HUD e atualizada via UpdateBuilding(nome, custo).

## 6. Fluxo de preview e validacao

[Assets/Scripts/Build/BuildPreviewController.cs](../Assets/Scripts/Build/BuildPreviewController.cs)

Etapas de UpdatePreview:
1. Lê posicao do mouse na tela (Input System).
2. Converte para mundo (Camera.main.ScreenToWorldPoint).
3. Converte para grid (GridManager.WorldToGridPosition).
4. Valida limites com GridManager.IsValidCell.
5. Move preview para centro da celula (GridManager.GridToWorldCenter).
6. Calcula canBuild com duas regras:
- GridManager.IsCellBuildable(x, y)
- GoldManager.CanAfford(CurrentBuilding.buildCost)
7. Define cor do preview:
- Verde translucido: valido.
- Vermelho translucido: invalido.

## 7. Fluxo de construcao

BuildPreviewController.TryBuild:
1. Aborta se canBuild for false.
2. Aborta se GoldManager.Spend(custo) falhar.
3. Instancia CurrentBuilding.prefab no centro da celula.
4. Se icon existir e o objeto tiver SpriteRenderer, aplica sprite da icon.
5. Registra ocupante em GridManager.SetCellOccupant.
6. Publica audio GameAudioEvents.RaiseBuildPlaced.

Efeito colateral esperado:
- GoldManager dispara OnGoldChanged para atualizar HUD de dinheiro.

## 8. Fluxo de destruicao

BuildPreviewController.TryDestroy:
1. Obtem ocupante da celula via GridManager.GetCellOccupant.
2. Aborta se nao houver ocupante.
3. Procura componente Buildable no ocupante.
4. Aborta se nao for Buildable ou se CanBeDestroyed for false.
5. Calcula e aplica reembolso em GoldManager.Add.
6. Destroi GameObject ocupante.
7. Limpa ocupacao da celula com SetCellOccupant(..., null).
8. Publica audio GameAudioEvents.RaiseBuildRemoved.

## 9. Integracoes

### 9.1 Grid

[Assets/Scripts/Grid/GridManager.cs](../Assets/Scripts/Grid/GridManager.cs)
- IsCellBuildable: exige celula valida, nao ocupada e buildavel.
- SetCellOccupant/GetCellOccupant: contrato de ocupacao de celula.
- WorldToGridPosition/GridToWorldCenter: sincronizacao preview <-> grade.

### 9.2 Economia

[Assets/Scripts/Systems/GoldManager.cs](../Assets/Scripts/Systems/GoldManager.cs)
- CanAfford(int): valida compra.
- Spend(int): debita e retorna sucesso/falha.
- Add(int): aplica reembolso.

### 9.3 HUD

[Assets/Scripts/UI/GameHUDController.cs](../Assets/Scripts/UI/GameHUDController.cs)
- UpdateMode(BuildMode): texto de modo atual.
- UpdateBuilding(string, int): texto de construcao e custo.

### 9.4 Audio

[Assets/Scripts/Systems/Audio/GameAudioEvents.cs](../Assets/Scripts/Systems/Audio/GameAudioEvents.cs)
- RaiseBuildPlaced(Vector3): evento de colocacao.
- RaiseBuildRemoved(Vector3): evento de remocao.

### 9.5 Entidades

[Assets/Scripts/Entities/DefenseTower.cs](../Assets/Scripts/Entities/DefenseTower.cs)
- Le o BuildingData atraves do componente Buildable para configurar tiro e efeitos.

## 10. Configuracao no inspector

No BuildPreviewController:
1. Referenciar GridManager, GoldManager e GameHUDController.
2. Definir previewPrefab com SpriteRenderer.
3. Preencher array buildings com BuildingData validos.
4. Ajustar selectedIndex inicial para um indice existente.

No prefab de construcao:
1. Garantir componente Buildable com referencia de BuildingData.
2. Para torres, garantir configuracao do DefenseTower (projectile, firePoint, collider/range).

## 11. API publica principal

### Buildable
- Data: retorna BuildingData associado.
- CanBeDestroyed: regra de remocao.
- GetRefundValue(): valor inteiro do reembolso.

### BuildingData
- Asset de configuracao consumido por build e por entidades (ex.: torres).

### GameHUDController (trechos usados pelo sistema de build)
- UpdateMode(BuildMode mode)
- UpdateBuilding(string buildingName, int cost = 0)

## 12. Convencoes e regras atuais

- Build em celula exige: dentro dos limites, celula buildavel e ouro suficiente.
- Remocao exige componente Buildable e canBeDestroyed true.
- Reembolso e truncado para baixo com Mathf.FloorToInt.
- Selecao de estruturas e ciclica com Q/E.
- Modo alterna com Tab e afeta o clique esquerdo.

## 13. Limitacoes atuais e pontos de atencao

- BuildPreviewController assume que o array buildings tem ao menos 1 item valido.
- BuildPreviewController assume previewPrefab com SpriteRenderer.
- UpdatePreview calcula canBuild mesmo em Destroy mode; a cor do preview representa validade de build, nao validade de destroy.
- TryBuild nao valida prefab nulo em BuildingData.
- Nao existe cooldown global para spam de colocacao/remocao por frame.

## 14. Pontos de extensao sugeridos

- Adicionar validacao defensiva para buildings vazio/nulo e prefabs invalidos.
- Exibir feedback visual dedicado para Destroy mode.
- Introduzir regras por tipo de celula (ex.: torres permitidas apenas em tipos especificos).
- Adicionar custo de remocao ou taxas por tipo de estrutura.
- Instrumentar eventos de analytics para build/destroy por celula/tipo.
