# Sistema de Grid - Documentacao Tecnica

## 1. Objetivo

Este documento descreve todo o sistema de grid do projeto, incluindo:
- Modelagem de dados da grade.
- Pipeline de criacao de mapas no Editor.
- Pipeline de carga e renderizacao em runtime.
- Integracoes com sessao de jogo, nivel e input.
- API publica principal e pontos de extensao.

## 2. Visao geral da arquitetura

Arquivos centrais:
- [Assets/Scripts/Grid/GridManager.cs](../Assets/Scripts/Grid/GridManager.cs)
- [Assets/Scripts/Data/GridCell.cs](../Assets/Scripts/Data/GridCell.cs)
- [Assets/Scripts/Data/CellType.cs](../Assets/Scripts/Data/CellType.cs)
- [Assets/Scripts/ScriptableObjects/GridMapAsset.cs](../Assets/Scripts/ScriptableObjects/GridMapAsset.cs)
- [Assets/Scripts/ScriptableObjects/GridTheme.cs](../Assets/Scripts/ScriptableObjects/GridTheme.cs)
- [Assets/Editor/GridMapAssetEditor.cs](../Assets/Editor/GridMapAssetEditor.cs)
- [Assets/Scripts/Grid/GridObject.cs](../Assets/Scripts/Grid/GridObject.cs)
- [Assets/Scripts/Systems/LevelData.cs](../Assets/Scripts/Systems/LevelData.cs)
- [Assets/Scripts/Systems/GameSession.cs](../Assets/Scripts/Systems/GameSession.cs)
- [Assets/Scripts/Input/GridClickerTester.cs](../Assets/Scripts/Input/GridClickerTester.cs)

Responsabilidades por classe:
- GridManager: orquestra criacao da grade, visual, spawn de entidades do mapa, caminho e enquadramento de camera.
- GridMapAsset: armazena dimensoes e dados de celulas do mapa.
- GridTheme: armazena sprites de visualizacao do mapa e auto-tiling de caminho.
- GridCell: estado runtime de cada celula (tipo, ocupacao, buildavel).
- GridMapAssetEditor: ferramenta de pintura do mapa no inspector.
- GridObject: componente para alinhamento de prefab em celula.
- GameSession + LevelData: definem qual mapa entra em gameplay.

## 3. Modelo de dados

### 3.1 Tipos de celula

[Assets/Scripts/Data/CellType.cs](../Assets/Scripts/Data/CellType.cs)
- Empty
- Path
- Blocked
- Tree
- Rock
- Spawn
- Goal

### 3.2 Estado runtime da celula

[Assets/Scripts/Data/GridCell.cs](../Assets/Scripts/Data/GridCell.cs)
- position: coordenada da celula (x, y).
- type: tipo da celula (CellType).
- isBuildable: atualmente true apenas para Empty.
- occupant: GameObject ocupante da celula (torre, nexus, spawn etc).
- Propriedades auxiliares: IsOccupied, IsPath, IsSpawn, IsGoal.

### 3.3 Asset de mapa

[Assets/Scripts/ScriptableObjects/GridMapAsset.cs](../Assets/Scripts/ScriptableObjects/GridMapAsset.cs)
- width e height: dimensoes logicas da grade.
- theme: referencia para GridTheme.
- cells: vetor linearizado com tamanho width * height.
- Indexacao: x + y * width.
- GetCell/SetCell: leitura e escrita da celula.
- Regra importante em SetCell:
- Garante apenas 1 Spawn no mapa.
- Garante apenas 1 Goal no mapa.
- Resize: recria o array de celulas conforme dimensoes.

### 3.4 Tema visual

[Assets/Scripts/ScriptableObjects/GridTheme.cs](../Assets/Scripts/ScriptableObjects/GridTheme.cs)
- Sprites base: empty, path, spawn, goal, blocked.
- Sprites de auto-tiling de caminho: straight, corner, T, cross, end.
- Cores fallback definidas no asset.
- Custom Cell Overrides: lista para configurar visual de novos tipos (sprite, color e sorting order) sem hardcode.

## 4. Pipeline de authoring (Editor)

[Assets/Editor/GridMapAssetEditor.cs](../Assets/Editor/GridMapAssetEditor.cs)

Fluxo no inspector do GridMapAsset:
1. Definir width, height e theme.
2. Clicar em Initialize / Resize Grid para criar ou redimensionar cells.
3. Escolher tipo de pintura (toolbar com CellType).
4. Pintar clicando e arrastando no grid desenhado no inspector.
5. Editor aplica Undo e marca o asset como dirty.

Comportamentos importantes:
- Se cells estiver nulo ou tamanho invalido, editor mostra aviso e para.
- Pintura usa SetCell, portanto regras de 1 Spawn e 1 Goal sao respeitadas.

## 5. Pipeline de runtime

### 5.1 Selecao de nivel

[Assets/Scripts/Systems/GameSession.cs](../Assets/Scripts/Systems/GameSession.cs)
- SelectLevel(LevelData) define SelectedLevel.
- Em seguida carrega cena GamePlay.

[Assets/Scripts/Systems/LevelData.cs](../Assets/Scripts/Systems/LevelData.cs)
- Cada nivel referencia um GridMapAsset em map.

### 5.2 Inicializacao do GridManager

[Assets/Scripts/Grid/GridManager.cs](../Assets/Scripts/Grid/GridManager.cs)
- Awake valida se existe GameSession e SelectedLevel com mapa.
- LoadMap inicializa estado do mapa e dispara construcao runtime.

### 5.3 Construcao runtime

Sequencia executada:
1. CreateGridFromMap: cria matriz GridCell[,] a partir de GridMapAsset.
2. RegisterSpecialCell: identifica spawnCell e goalCell.
3. GenerateGridVisual: cria camadas de tiles (ground/path e conteudo por tipo usando GridTheme).
4. SpawnMapEntities: instancia enemy spawn, nexus e prefabs vinculados por tipo de celula.
5. CenterCamera: centraliza e ajusta zoom para caber grid.
6. DebugPath: monta e loga caminho percorrido.

## 6. Camadas visuais da grade

No GridManager existem 3 matrizes de render:
- groundLayer
- pathLayer
- contentLayer

Cada celula pode gerar:
- Ground sempre.
- Path se tipo for Path.
- Conteudo para qualquer outro tipo (Blocked, Spawn, Goal, Tree, Rock e futuros), usando mapeamento do GridTheme.

Criacao de tile:
- Usa prefab gridTilePrefab.
- Nomeia objeto por tipo e coordenada.
- Ajusta SpriteRenderer (sprite + sortingOrder).
- Se houver GridObject, chama AlignToGrid(cellSize).

## 7. Entidades de mapa e ocupacao

No GridManager:
- SpawnEnemySpawnPoint instancia enemySpawnPrefab em spawnCell.
- SpawnNexus instancia nexusPrefab em goalCell.
- SpawnBoundCellPrefabs instancia prefabs vinculados a tipos de celula via Cell Type Prefabs.
- SpawnEntityAtCell pode registrar ocupacao da celula com SetCellOccupant.

Evento:
- OnGridChanged dispara quando SetCellOccupant altera ocupante.

## 8. Caminho e auto-tiling

### 8.1 Auto-tiling visual do path

GetPathSprite:
1. Lê vizinhos cardinais via IsPath.
2. Conta conexoes.
3. Escolhe sprite no GridTheme:
- 4 conexoes: cross
- 3 conexoes: T
- 2 opostas: straight
- 2 adjacentes: corner
- caso contrario: end

### 8.2 Montagem do caminho logico

BuildPath:
- Comeca em spawnCell.
- Avanca por celulas Path/Goal nao visitadas.
- Para quando chega em goalCell.
- Se travar, loga erro de caminho invalido.

Observacao:
- Implementacao atual presume um caminho linear sem bifurcacoes ambiguas.

## 9. Conversao de coordenadas

No GridManager:
- WorldToGridPosition converte coordenada de mundo para celula (floor por cellSize).
- GridToWorldCenter converte celula para centro no mundo.

## 10. Camera adaptativa

No GridManager:
- Detecta mudanca de resolucao em Update.
- Recalcula posicao e orthographicSize da camera.
- Ajuste considera proporcao tela vs proporcao da grade.

## 11. Input de teste

[Assets/Scripts/Input/GridClickerTester.cs](../Assets/Scripts/Input/GridClickerTester.cs)
- Captura clique esquerdo do mouse via Input System.
- Converte para mundo e depois para grid.
- Loga celula clicada se valida.

## 12. API publica do GridManager

Metodos publicos:
- LoadMap(GridMapAsset map): carrega e constroi runtime da grade.
- IsValidCell(int x, int y): valida limite.
- IsCellBuildable(int x, int y): valida limite, ocupacao e regra de construcao.
- SetCellOccupant(Vector2Int pos, GameObject occupant): registra ocupacao e emite evento.
- GetCellOccupant(Vector2Int pos): consulta ocupante.
- BuildPath(): retorna caminho do spawn ao goal.
- WorldToGridPosition(Vector3 worldPosition): conversao world->grid.
- GridToWorldCenter(Vector2Int gridPos): conversao grid->world.

## 13. Convencoes e regras atuais

- Celula buildavel por padrao: apenas Empty.
- Spawn e Goal unicos por mapa (regra no GridMapAsset).
- Cenario esperado de caminho: conectividade simples entre Spawn e Goal.
- Ocupacao de celula centralizada em SetCellOccupant.
- Prefab por tipo e configurado no inspector do GridManager, em Cell Type Prefabs.

## 14. Limitacoes atuais e pontos de atencao

- GridObject.AlignToGrid esta com escala comentada, sem ajuste efetivo de tamanho no momento.
- ApplyTheme existe no GridManager, mas nao esta sendo usado no fluxo atual de render.
- LoadMap nao limpa instancias antigas se chamado multiplas vezes no mesmo ciclo (avaliar se necessario para runtime dinamico).
- BuildPath usa estrategia gulosa por vizinho e nao e um algoritmo geral de pathfinding.

## 15. Recomendacoes de evolucao

Separacao por arquivos (composicao) para escalar manutencao:
- GridRuntimeBuilder: construcao de GridCell e celulas especiais.
- GridVisualizer: geracao de tiles e auto-tiling.
- GridPathService: regras de caminho e iteracao.
- GridCameraFitter: posicionamento e zoom da camera.
- GridManager: somente coordenacao e API de alto nivel.

Beneficios esperados:
- Testabilidade melhor.
- Menor acoplamento.
- Menos risco de regressao ao evoluir features.

## 16. Checklist rapido para criar novo mapa

1. Criar GridTheme e preencher sprites.
2. Criar GridMapAsset, definir dimensoes e theme.
3. Inicializar grid no editor customizado.
4. Pintar Path, Spawn e Goal.
5. Criar LevelData e apontar map.
6. Garantir GameSession seleciona o LevelData antes da cena de gameplay.
7. Validar em runtime: tiles, spawn, nexus, camera e BuildPath.

## 17. Como criar novo tipo e vincular prefab

Exemplo: criar tipos de cenario como arvore ou pedra.

Passo 1. Adicionar o tipo no enum
- Arquivo: [Assets/Scripts/Data/CellType.cs](../Assets/Scripts/Data/CellType.cs)
- Adicione um novo valor no enum, por exemplo `Tree`, `Rock`, `Bush`, `Water`.

Passo 2. Configurar visual do tipo no tema
- Arquivo: [Assets/Scripts/ScriptableObjects/GridTheme.cs](../Assets/Scripts/ScriptableObjects/GridTheme.cs)
- No asset do tema usado pelo mapa, em Custom Cell Overrides, adicione um item:
- type: novo CellType.
- sprite: sprite do bloco.
- color: cor usada no editor de pintura.
- sortingOrder: ordem de render do tile desse tipo.

Passo 3. Pintar o novo tipo no mapa
- Arquivo: [Assets/Editor/GridMapAssetEditor.cs](../Assets/Editor/GridMapAssetEditor.cs)
- O novo tipo aparece automaticamente no Paint Mode.
- Pinte as celulas desejadas.

Passo 4. Vincular prefab que deve spawnar
- Arquivo: [Assets/Scripts/Grid/GridManager.cs](../Assets/Scripts/Grid/GridManager.cs)
- No inspector do objeto com GridManager, em Cell Type Prefabs, adicione um binding:
- type: o novo CellType.
- prefab: prefab a ser instanciado naquela celula.
- occupyCell: se marcado, celula fica ocupada no runtime.
- parentToGrid: se marcado, instancia como filho do GridManager.

Passo 5. Validar em runtime
- Inicie o nivel que usa o mapa.
- Confira se o tile visual do tipo novo aparece.
- Confira se o prefab e instanciado nas celulas desse tipo.
- Se occupyCell estiver ligado, valide que GetCellOccupant retorna o objeto.

Observacoes:
- Spawn e Goal continuam com comportamento dedicado (enemy spawn e nexus), alem do visual da celula.
- Se um tipo nao tiver override no tema, o editor usa cor fallback gerada e o runtime so mostrara tile se houver sprite resolvido.
