# HoloTactics: Juego de Estrategia de Cartas Holográficas

## 1. Introducción y Justificación
Este proyecto consiste en el desarrollo de un videojuego de estrategia por turnos en 3D desarrollado en **Unity**. La propuesta nace de la intención de combinar la lógica clásica de los juegos de cartas (tipo *Hearthstone*) con una estética futurista y tablero físico, poniendo especial énfasis en el **feedback visual** y la **arquitectura de software**.

El objetivo principal no es solo crear un juego funcional, sino implementar un ciclo de juego robusto (Core Loop) que maneje estados complejos entre el Jugador y una Inteligencia Artificial.

## 2. Tecnologías Utilizadas
* **Motor:** Unity 2022.3 (LTS).
* **Lenguaje:** C# (.NET Standard).
* **Render Pipeline:** Universal Render Pipeline (URP) para optimización en equipos de bajos recursos.
* **Herramientas Gráficas:** Shader Graph (para el efecto holográfico) y Particle System.
* **UI/Texto:** TextMeshPro (versiones UI y World Space).
* **Control de Versiones:** Git & GitHub.

## 3. Arquitectura del Proyecto
El código sigue una estructura organizada para separar la lógica de la representación visual:

* **ControladorPartida (Game Manager):** Es el cerebro del juego. Gestiona:
    * El control de turnos (Jugador vs IA).
    * La lógica de combate secuencial (`IEnumerator`).
    * La instanciación del tablero y las cartas.
* **Modelos de Datos (ModeloCriatura, ModeloTablero):** Clases puras de C# que contienen los datos (vida, ataque, coste) sin depender de Unity.
* **Vistas (VistaCriatura, InfoCasilla):** Scripts `MonoBehaviour` encargados de pintar lo que ocurre: mover objetos, lanzar partículas y actualizar textos.

## 4. Retos Técnicos y Soluciones
Durante el desarrollo surgieron varios desafíos críticos que se resolvieron mediante programación avanzada:

### A. El Problema de los "Zombis" (Unidades muertas atacando)
**Dificultad:** Las unidades que morían durante una fase de combate seguían existiendo en la memoria hasta el final del bucle, provocando que atacaran con vida negativa.
**Solución:** Se implementó una verificación de integridad en el bucle de combate dentro de `ControladorPartida`. Además, se separó la lógica de datos de la visual, asegurando que `LimpiarCasilla()` elimine la referencia lógica instantáneamente antes de destruir el objeto visual.

### B. Sincronización de Turnos y Animaciones
**Dificultad:** Al principio, todos los ataques ocurrían en el mismo *frame*, haciendo imposible leer la acción.
**Solución:** Se transformó el método `ResolverFaseCombate` de `void` a `IEnumerator`. Esto permitió usar `yield return new WaitForSeconds(0.4f)` entre cada ataque, creando una secuencia dramática y legible donde los golpes ocurren uno tras otro.

### C. Visibilidad de Textos en World Space
**Dificultad:** Los indicadores de daño (TextMeshPro) eran invisibles o se renderizaban detrás de los hologramas debido al Z-Buffer.
**Solución:** Se configuró el shader de los textos en modo `Overlay` y se implementó un script `TextoFlotante` que invierte el vector de dirección hacia la cámara (`LookRotation`) para asegurar que el texto siempre mire al jugador, independientemente de la rotación de la cámara.

## 5. El Shader Holográfico
Uno de los puntos fuertes es el aspecto visual. Se diseñó un shader personalizado en **Shader Graph** que incluye:
* **Efecto Fresnel:** Para iluminar los bordes de las cartas simulando luz proyectada.
* **Scanlines:** Un patrón de líneas que se desplaza con el tiempo (`Time` node) para simular interferencia digital.
* **Transparencia Aditiva:** Para que los hologramas se "sumen" a la luz del ambiente sin generar sombras duras.

## 6. Conclusiones y Futuras Mejoras
El proyecto cumple con todos los requisitos de un MVP (Producto Mínimo Viable). Se ha logrado una IA capaz de evaluar columnas amenazadas y defenderse. 
Como líneas futuras de trabajo se plantea:
* Implementación de un sistema de construcción de mazos (Deck Builder).
* Modo multijugador en red local.
* Nuevos tipos de cartas (Hechizos instantáneos).