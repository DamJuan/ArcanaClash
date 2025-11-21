# 📅 Hoja de Ruta: Tareas Pendientes para Arcana Clash

Este documento detalla las funcionalidades y mejoras visuales restantes para completar el MVP (Producto Mínimo Viable) del proyecto.

## 1. Arte y Entorno (Visuals)
Mejoras estéticas para integrar la jugabilidad en un entorno coherente.

- [ ] **Integración del Tablero en Entorno 3D:**
    - Sustituir la plataforma base actual por un *asset* de **Mesa de Juego** (estilo taberna o local de juegos).
    - Integrar las casillas generadas (`PrefabCasilla`) sobre la superficie de dicha mesa.
- [ ] **Mapa y Ambientación:**
    - Implementar un entorno ("skybox" o modelo 3D de habitación) que envuelva la mesa de juego.
    - Asegurar que la iluminación favorezca la lectura de las cartas sobre la mesa.

## 2. Gestión de Mazos y Mano (Core Mechanics)
Lógica interna de las colecciones de cartas.

- [ ] **Sistema de Baraja (Deck):**
    - Crear una pila de cartas (`Stack` o `List`) oculta de la que el jugador roba.
    - Implementar la lógica de **Mazo Vacío**: Cuando no quedan cartas, se deshabilita la acción de robar (o se aplica penalización/derrota).
- [ ] **Limitaciones de la Mano:**
    - Implementar un límite máximo de **5 cartas en mano**.
    - Si el jugador tiene 5 cartas, la carta robada se quema (se descarta) o no se permite robar.

## 3. Sistema de Turnos (Game Loop)
Control del flujo de la partida entre el Jugador y la IA.

- [ ] **Fases del Turno:**
    1.  **Inicio:** Robo automático de 1 carta.
    2.  **Acción (Main Phase):** Jugar cartas (colocar en tablero).
    3.  **Batalla:** Las criaturas atacan.
    4.  **Fin:** Pasar el turno al oponente.
- [ ] **Restricciones de Acción:**
    - Limitar el número de cartas jugadas por turno a un **máximo de 4**.
    - Implementar contador visual de cartas jugadas en el turno actual.

## 4. Lógica de Combate
Reglas específicas de las criaturas en el tablero.

- [ ] **Mareo de Invocación (Summoning Sickness):**
    - Las cartas recién colocadas entran en estado "Descanso".
    - No pueden atacar ni activar habilidades hasta el inicio del **siguiente turno** del jugador.

## 5. Inteligencia Artificial (IA Oponente)
Comportamiento del rival para el modo un jugador.

- [ ] **IA Básica (Reglas Espejo):**
    - La IA debe respetar las mismas reglas que el jugador:
        - Robar al inicio.
        - Tener límite de mano (5) y límite de jugadas (4).
        - Solo colocar cartas en su territorio (filas superiores).
    - **Toma de decisiones:**
        - Detectar casillas vacías en su territorio.
        - Seleccionar una carta aleatoria (o la de mayor coste posible) de su mano y jugarla.
        - Pasar turno al finalizar sus acciones.