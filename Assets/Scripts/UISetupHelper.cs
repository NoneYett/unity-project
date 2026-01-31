using UnityEngine;
using UnityEngine.UI;

public class UISetupHelper : MonoBehaviour
{
    [Header("Essa classe ajuda a configurar a UI no Unity Editor")]
    [TextArea(10, 20)]
    public string instrucoes = @"
=== INSTRUÇÕES DE CONFIGURAÇÃO DA UI ===

1. MENU PRINCIPAL (Cena 'MainMenu'):
   - Criar Canvas (UI > Canvas)
   - Criar Panel para fundo escuro
   - Criar textos para título do jogo
   - Criar botões: 'Jogar', 'Opções', 'Sair'
   - Adicionar script MainMenu.cs ao Canvas
   - Configurar OnClick dos botões

2. HUD DO JOGO (Cena 'Supermarket'):
   - Criar Canvas com modo Screen Space - Overlay
   - Adicionar script GameUI.cs ao Canvas
   
   Elementos do HUD:
   - Crosshair (Image no centro, pequeno círculo branco)
   - Texto de preço total (canto superior direito)
   - Texto de interação (centro inferior)
   - Painel de lista de compras (canto superior esquerdo)

3. MENU DE PAUSE:
   - Criar Panel com fundo escuro semi-transparente
   - Botões: 'Continuar', 'Reiniciar', 'Menu Principal', 'Sair'
   - Adicionar script PauseMenu.cs ao Canvas
   - Conectar o Panel ao campo 'pausePanel'

4. CHECKOUT:
   - Criar zona de trigger no caixa (Box Collider com isTrigger = true)
   - Criar Panel para tela de checkout
   - Textos: Total, Quantidade de itens, Pontuação
   - Botões: 'Finalizar', 'Continuar Comprando'
   - Adicionar script CheckoutManager.cs à zona de trigger

5. PREFABS NECESSÁRIOS:
   - ShoppingListItem: Panel com TextMeshPro (para itens da lista)

6. TAGS NECESSÁRIAS:
   - 'Produto' - nos produtos
   - 'ShoppingCart' - no carrinho
   - 'CartHandle' - na alça do carrinho

7. LAYERS:
   - 'Player' - no jogador (para ignorar raycast)

=== ÁUDIO ===
Para adicionar sons:
1. Criar objeto vazio 'AudioManager'
2. Adicionar 2 AudioSource (música e SFX)
3. Adicionar script AudioManager.cs
4. Arrastar os clipes de áudio para os campos

=== CENAS NO BUILD ===
Em File > Build Settings, adicionar:
1. MainMenu (índice 0)
2. Supermarket (índice 1)
";

    void Start()
    {
        Debug.Log("UISetupHelper: Veja o campo 'instrucoes' no Inspector para guia de configuração!");
    }
}
