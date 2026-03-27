# Como Vincular Sprites a Tipos de Celula no Grid Theme

## Objetivo

Este guia ensinacomo configurar sprites para tipos de celula customizados (como Tree, Rock) no sistema de grid.

## O que voce vai precisar

1. Um GridMapAsset ja criado (veja [grid-system.md](grid-system.md)).
2. Um GridTheme atribuido ao mapa.
3. Sprites prontos importados no Unity.

## Passo a passo

### 1. Identifique o tipo de celula que quer customizar

No arquivo [Assets/Scripts/Data/CellType.cs](../Assets/Scripts/Data/CellType.cs), o enum inclui:
- Empty
- Path
- Blocked
- Tree
- Rock
- Spawn
- Goal

Se voce quer vincular sprite para Tree, Rock ou qualquer tipo nao-padrao, continue.

### 2. Selecione o GridTheme a usar

Navegue ate o seu GridMapAsset no inspector. No campo theme, ja deve estar atribuido um GridTheme. Se nao estiver, crie um:
- Right-click em uma pasta em Assets.
- Create > Grid > Grid Theme.

### 3. Configure o sprite no GridTheme

Abra o GridTheme (clique no asset ou no campo theme do GridMapAsset).

No inspector do GridTheme, procure a secao Custom Cell Overrides:

```
Custom Cell Overrides
[+] (botao para adicionar novo item)
```

#### Passo 3.1: Adicione um novo override

Clique no botao + para adicionar um novo item na lista.

#### Passo 3.2: Configure o tipo

Na lista, encontre o novo item e abra-o (clique na seta de expansao).

Defina o campo **type**:
- Clique no dropdown de CellType.
- Selecione **Tree** (ou Rock, ou o tipo customizado que voce quer).

#### Passo 3.3: Atribua o sprite

Localize o campo **sprite** no override:
- No Project, encontre o sprite que quer usar.
- Arraste o sprite para o campo sprite do override, OU
- Clique no circulo de seletor e escolha o sprite da lista.

#### Passo 3.4 (Opcional): Ajuste sorting order e cor

- **sortingOrder**: Controla a profundidade visual. Use 2 ou maior para aparecer acima do ground (que comeca em 0).
- **color**: Deixe branco (1, 1, 1, 1) por padrao, ou customize se quiser tint.

Exemplo de valores:
- Tree: sortingOrder = 2
- Rock: sortingOrder = 2
- Path: sortingOrder = 1
- Ground: sortingOrder = 0

### 4. Pinte o mapa com o novo tipo

Abra o GridMapAsset no inspector.

Procure a secao editor (abaixo de cells):
- Clique em **Initialize Grid** ou **Resize** se nunca tiver feito.
- Na toolbar de tipos, selecione **Tree**.
- Clique e arraste no grid para pintar celulas como Tree.

### 5. Rode a cena

Execute a cena de gameplay. O GridManager ira:
1. Ler as celulas do tipo Tree do mapa.
2. Consultar o GridTheme para encontrar o sprite customizado.
3. Instanciar tiles visuais com o sprite que voce atribuiu.

## Roteiros comuns

### Adicionar arvores, pedras e plantas

Crie 3 Custom Cell Overrides no GridTheme:
1. type = Tree, sprite = TreeSprite, sortingOrder = 2
2. type = Rock, sprite = RockSprite, sortingOrder = 2
3. (Opcional) type = Mountain, sprite = MountainSprite, sortingOrder = 3

Depois pinte o mapa livremente com cada tipo via editor do GridMapAsset.

### Customizar apenas a cor de um tipo

Se voce ja tem um sprite e quer mudar a cor:
1. Defina o sprite no override.
2. Ajuste o campo color (ex.: verde escuro para arvore).

O GridManager ignora a cor visual no fluxo atual, mas ela fica disponivel para futuras extensoes.

### Evitar conflitos de profundidade

A ordem padrao recomendada:
- Ground: sortingOrder = 0
- Path: sortingOrder = 1
- Blocked, Tree, Rock, etc.: sortingOrder = 2
- Spawn, Goal: sortingOrder = 3

Se arvores devem ficar atras de towers, mantenha Tree em 2 e torres em 3+.

## Troubleshooting

**O sprite nao aparece apesar de estar configurado:**
- Confirme que o tipo foi pintado no mapa (check no GridMapAsset.cells).
- Verifique se o GridTheme esta corretamente atribuido ao mapa.
- Valide que o sprite nao e nulo no override.

**O tipo aparece, mas com a cor errada:**
- A cor visual no override ainda nao e usada no fluxo. Customize a cor do sprite no asset mesmo.

**Nenhuma celula visivel:**
- Certifique-se de que o GridManager conseguiu carregar o mapa (check console para erros).

## Referencias

- [Grid System - Arquitetura Completa](grid-system.md#4-pipeline-de-authoring-editor)
- [GridTheme.cs - Implementacao de resolucao de sprites](../Assets/Scripts/ScriptableObjects/GridTheme.cs)
- [GridManager.cs - Consumo de sprites em runtime](../Assets/Scripts/Grid/GridManager.cs#L209)
