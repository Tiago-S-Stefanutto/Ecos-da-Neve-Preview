# Ecos da Neve Preview - Instruções de Configuração

Este projeto foi estruturado para o **Unity 6 (LTS)**. Siga os passos abaixo para configurar o ambiente corretamente:

## 1. Pacotes Necessários
Certifique-se de instalar os seguintes pacotes via Package Manager:
- **Universal RP** (Configurar para 2D)
- **Cinemachine** (Para o sistema de câmera)
- **2D Tilemap Editor**
- **TextMeshPro**

## 2. Configuração de Tags e Camadas
Crie as seguintes Tags e Layers:
- **Tag:** `Player` (Atribuir ao prefab do Jogador)
- **Layer:** `Ground` (Atribuir ao Tilemap de colisão)

## 3. Configuração do Player
O prefab do jogador deve ter:
- `Rigidbody2D` (Dynamic, Collision Detection: Continuous)
- `CapsuleCollider2D`
- Script `CelestePlayerController`
- Objeto filho `Skin` com `SpriteRenderer` e `Animator`
- Objeto filho `GroundCheck` posicionado nos pés

## 4. Configuração de Áudio
- O `AudioManager` utiliza um `AudioMixer`. Crie um Mixer com os grupos: `Master`, `Music` e `SFX`.
- Exponha os parâmetros de volume como `MasterVol`, `MusicVol` e `SFXVol`.

## 5. Cenas
- **MainMenu:** Configure os botões para chamar as funções no script `MainMenuUI`.
- **DemoLevel:** Utilize o `GameManager` para gerenciar o respawn e o `Cinemachine` para seguir o jogador.

## 6. Layout da Fase (DemoLevel)
A fase foi planejada em 4 seções:
1. **Introdução:** Pulos simples e aprendizado de wall jump.
2. **Verticalidade:** Escalada e plataformas estreitas.
3. **Dash:** Uso obrigatório do dash para atravessar espinhos.
4. **Precisão:** Sequências complexas combinando todas as mecânicas.

---
Desenvolvido por Manus AI.
