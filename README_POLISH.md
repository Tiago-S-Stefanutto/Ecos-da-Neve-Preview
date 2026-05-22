# Ecos da Neve - Guia de Polish & Vertical Slice

Para alcançar a qualidade "Premium Indie", siga estas diretrizes ao configurar o projeto no Unity 6:

## 1. Direção de Arte (Lighting & Post-Processing)
- **Global Volume:** Adicione um Volume Global com:
    - **Bloom:** Intensidade 0.5, Threshold 1.0 (para brilho na neve e dash).
    - **Color Grading:** Use um LUT azulado ou ajuste a Temperatura para -20.
    - **Vignette:** Intensidade 0.3 para focar no centro.
    - **Film Grain:** Intensidade 0.1 para textura cinematográfica.
- **2D Lights:** 
    - Use **Global Light 2D** com cor azul claro suave (Intensidade 0.4).
    - Use **Point Light 2D** no jogador e em cristais/pontos de interesse.

## 2. Game Feel (Juice)
- **Squash & Stretch:** O script `JuiceEffects` deve ser configurado no objeto do Jogador.
- **Freeze Frame:** O dash agora pausa o tempo por 0.05s para impacto.
- **Ghost Trail:** Configure o prefab `GhostPrefab` (SpriteRenderer com material transparente) no script `GhostEffect`.

## 3. Level Design (Vertical Slice)
- **Seção 1 (O Silêncio):** Inicie com apenas o som do vento e passos na neve.
- **Seção 2 (As Ruínas):** Introduza o dash para atravessar arcos de pedra congelados.
- **Seção 3 (A Fuga):** Utilize o `AvalancheTrigger` para criar uma perseguição onde o jogador deve usar wall jumps e dashes precisos.
- **Seção 4 (O Cume):** Termine em uma área aberta com vista para as montanhas e a mensagem de agradecimento.

## 4. Áudio Imersivo
- **Ambiente:** Loop de vento constante no `WindManager`.
- **Passos:** Use o sistema de animação para disparar eventos de áudio de "crunch" na neve.
- **Música:** Inicie com um piano melancólico e transicione para algo mais tenso durante a avalanche.

## 5. Pixel Perfect
- No **Universal Render Pipeline Asset**, habilite o **Upscaling Filter: Edge Adaptive** ou use o componente **Pixel Perfect Camera** na Main Camera para garantir que os pixels não "quebrem" durante o movimento suave.

---
Este guia complementa o `README_SETUP.md` inicial.
