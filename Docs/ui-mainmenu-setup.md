# Menu Principal - Guia de Implementacao

## Arquivos

- [Assets/Settings/UI/MainMenu.uxml](../Assets/Settings/UI/MainMenu.uxml) - Layout do menu
- [Assets/Scripts/UI/MainMenuController.cs](../Assets/Scripts/UI/MainMenuController.cs) - Controlador de navegação
- [Assets/Scripts/Systems/LevelManager.cs](../Assets/Scripts/Systems/LevelManager.cs) - Gerenciador de níveis

## Estrutura UXML

O arquivo `MainMenu.uxml` contem tres secoes principais:

### 1. MainMenuSection (Menu Principal)
- Botao "Jogar": Navega para LevelsSection
- Botao "Opcoes": Navega para OptionsSection
- Botao "Sair": Fecha o jogo

### 2. LevelsSection (Selecao de Niveis)
- Grid de botoes de nivel (gerados dinamicamente pelo LevelManager)
- Botao "Voltar" para retornar ao menu principal
- ScrollView para acomodar muitos niveis

### 3. OptionsSection (Opcoes)
- **Subsecao Controles**: Lista de teclas e acoes (Q/E, TAB, Clique, ESC)
- **Subsecao Volume**: Sliders para Musica e SFX com labels percentuais

## Implementacao Pronta

### MainMenuController.cs

Script MonoBehaviour que controla toda a logica de navegacao e eventos do menu:

**Responsabilidades:**
- Registra callbacks de botoes automaticamente
- Navega entre secoes (MainMenu → Levels → Options)
- Popula grid de niveis dinamicamente usando LevelManager
- Gerencia expansao/colapso das subsecoes de opcoes
- Atualiza volume via AudioManager

**Como usar:**
1. Crie um GameObject com UIDocument que carregue MainMenu.uxml
2. Adicione o script MainMenuController nesse GameObject
3. Atribua referencia a LevelManager no inspector
4. A referencia a UIDocument sera capturada automaticamente

### LevelManager.cs

ScriptableObject que centraliza a lista de niveis:

**Responsabilidades:**
- Armazena array de LevelData
- Fornece acesso por indice ou por ID
- Fornece contagem total de niveis

**Como usar:**
1. Right-click em Assets > Create > Game > Level Manager
2. Arraste LevelData assets existentes para o array de levels
3. Coloque LevelManager no inspector do MainMenuController

## Setup no Unity

### Passo 1: Criar LevelManager

- Create > Game > Level Manager
- Nomeie como "MainLevelsConfig" ou similar
- Preencha o array com os LevelData que existem no projeto
- Salve em Assets/Resources ou Assets/Settings

### Passo 2: Configurar UIDocument com MainMenu

- Crie um Canvas ou use existente
- Adicione UIDocument component
- Atribua MainMenu.uxml no campo PanelSettings/Panel

### Passo 2.5: Configurar AudioManager na cena do menu

**IMPORTANTE: Se os sliders de volume não funcionarem, o AudioManager pode não estar na cena.**

Opção A (Recomendado - AudioManager Singleton):
- Crie um GameObject vazio chamado "AudioManager"
- Adicione o script AudioManager nesse GameObject (ele já tem DontDestroyOnLoad)
- Configure os Sound IDs no inspector
- Coloque esse GameObject na cena do menu

Opção B (AudioManager em cena separada):
- Se AudioManager está em cena diferente, garanta que foi instanciado antes
- Use GameSession.Instance ou outro mecanismo para garantir que AudioManager existe

### Passo 3: Adicionar MainMenuController

- No mesmo GameObject do UIDocument, adicione o script MainMenuController
- No inspector, arraste o LevelManager criado no campo levelManager

### Passo 4: Testar

- Play a cena
- Navegue Jogar → ver niveis
- Clique em um nivel para carregar a cena "GamePlay"
- Volte e teste Opcoes

**Troubleshooting - Menu de Volume não funciona:**

Se o menu de volume abrir mas não funcionar:
1. Abra o Console (Window > General > Console)
2. Procure por logs:
   - Se ver "Slider de música registrado com sucesso" → Problema está no AudioManager
   - Se ver "Slider de música não encontrado" → Problema está no UXML
   - Se ver "AudioManager não disponível" → Problema está no setup do AudioManager

Solução:
- Garanta que AudioManager existe na cena ou em um Singleton pronto
- Se usando Singleton, verifique se Instance está sendo inicializado no Awake
- Confirme que o AudioManager tem DontDestroyOnLoad configurado (agora incluído automaticamente)

## Implementacao Futura

Se precisar customizar mais, o MainMenuController ja foi estruturado para permitir extensoes:

### Adicionar mais subsecoes em Opcoes

No UXML, crie novo container em OptionsSection e no C#, crie novo metodo:
```csharp
private void OnGraphicsButtonClicked()
{
    var details = root.Q("GraphicsDetails");
    details.style.display = details.style.display == DisplayStyle.None ? DisplayStyle.Flex : DisplayStyle.None;
}
```

### Persistir volumes em PlayerPrefs

No MainMenuController, em OnMusicVolumeChanged:
```csharp
PlayerPrefs.SetFloat("MusicVolume", value / 100f);
PlayerPrefs.Save();
```

### Criptografar/Desencriptar dados de save

Extenda LevelManager com metodos de persistencia de progress.

## Referencias

- [GameSession.cs](../Assets/Scripts/Systems/GameSession.cs) - Singleton global de sessao
- [AudioManager.cs](../Assets/Scripts/Systems/Audio/AudioManager.cs) - Gerenciador de audio com volume
- [LevelData.cs](../Assets/Scripts/Systems/LevelData.cs) - Dados de um nivel individual

## Campos e Nomes (referencia para C# - se customizar)

| Elemento | Name | Tipo |
|----------|------|------|
| Menu Principal | MainMenuSection | VisualElement |
| Jogar | PlayButton | Button |
| Opcoes | OptionsButton | Button |
| Sair | QuitButton | Button |
| Secao Niveis | LevelsSection | VisualElement |
| Grid de Niveis | LevelsGrid | VisualElement |
| Secao Opcoes | OptionsSection | VisualElement |
| Detalhes Teclas | KeysDetails | VisualElement |
| Detalhes Volume | VolumeDetails | VisualElement |
| Slider Musica | MusicVolumeSlider | SliderInt |
| Slider SFX | SfxVolumeSlider | SliderInt |
