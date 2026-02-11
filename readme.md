# ArcanaClash: Estrategia de Cartas Holográficas en 3D

## 1. Justificación y Objetivos

**ArcanaClash** es un videojuego de estrategia por turnos desarrollado en **Unity**. La idea surge de la ambición de aplicar los conocimientos de **Programación Orientada a Objetos (POO)** adquiridos durante el ciclo en un entorno complejo y visual como es un motor de videojuegos.

El objetivo principal no era crear un simple juego de cartas, sino fusionar la lógica de gestión de recursos (tipo *Hearthstone*) con el posicionamiento táctico espacial (tipo ajedrez), todo bajo una estética de "tablero holográfico futurista".

### Objetivos Específicos:
* Implementar un **Core Loop** (ciclo de juego) sólido: Gestión de turnos, maná y fases de combate.
* Desarrollar una **Inteligencia Artificial (IA)** capaz de tomar decisiones tácticas básicas.
* Separar la lógica de la interfaz mediante una arquitectura limpia (**MVC**).
* Investigar nuevas herramientas gráficas como **Shader Graph** para el feedback visual.

## 2. Tecnologías y Herramientas

Para el desarrollo se han utilizado herramientas actuales y estándares en la industria, combinando lo aprendido en el ciclo con autoaprendizaje:

* **Motor:** Unity 2022.3 LTS (Renderizado URP).
* **Lenguaje:** C# (.NET Standard).
* **Gestión de Datos:** Archivos CSV para "balancear" las estadísticas de las cartas externamente.
* **Gráficos:**
  * **Shader Graph:** Para crear el material holográfico (efecto Fresnel y líneas de barrido).
  * **Particle System:** Para los efectos de destrucción de unidades.
* **Interfaz (UI):** TextMeshPro para textos en pantalla y en el espacio 3D (World Space).
* **Control de Versiones:** Git y GitHub para la gestión del repositorio y control de cambios.

## 3. Metodología y Arquitectura

He seguido una metodología iterativa, desarrollando primero las mecánicas base (tablero y cartas) y añadiendo capas de complejidad (combate, IA, efectos visuales) progresivamente.

Para la arquitectura, he adaptado el patrón **Modelo-Vista-Controlador (MVC)**:

* **Modelo (Lógica Pura):** Clases de C# (`ModeloCriatura`, `ModeloTablero`) que solo contienen datos. No dependen de Unity, lo que facilita la gestión de la información.
* **Vista (Visual):** Scripts (`VistaCarta`, `InfoCasilla`) que solo se encargan de mover objetos, reproducir animaciones y mostrar textos.
* **Controlador (Cerebro):** `ControladorPartida`, un Singleton que gestiona el flujo, los turnos y arbitra las reglas.

## 4. Dificultades Encontradas y Soluciones

Durante el desarrollo surgieron varios retos técnicos que requirieron investigación y soluciones específicas:

### A. El Problema de la Sincronización (Corrutinas)
* **Dificultad:** Inicialmente, la fase de combate era instantánea. El usuario no veía quién atacaba a quién porque todos los cálculos ocurrían en el mismo fotograma.
* **Solución:** Convertí la función de combate en una **Corrutina** (`IEnumerator`). Esto me permitió introducir pausas de tiempo (`WaitForSeconds`) entre cada ataque, creando una secuencia dramática y legible donde los golpes ocurren uno tras otro.

### B. El Bug de los "Zombis" (Unidades muertas atacando)
* **Dificultad:** Detecté un error crítico donde una unidad que moría durante el combate seguía atacando si era su turno en el bucle `for`, llegando a tener vida negativa (-6).
* **Solución:** Implementé una verificación de integridad de datos antes de cada ataque. Además, separé la muerte lógica (borrar datos) de la visual (animación), asegurando que una unidad "muerta" deje de existir para el sistema inmediatamente, aunque su animación de explosión siga reproduciéndose.

### C. Visibilidad de Textos en 3D
* **Dificultad:** Los indicadores de daño flotantes eran invisibles o se veían "en espejo" al girar la cámara.
* **Solución:**
  1.  Configuré el material del texto en modo **Overlay** para que siempre se renderice por encima de los modelos 3D.
  2.  Programé un script que invierte el vector de dirección hacia la cámara, asegurando que los números siempre miren al jugador correctamente.

## 5. Estudio de Alternativas y Mejoras Futuras

El proyecto actual es un **MVP** (Producto Mínimo Viable) funcional. Se valoraron otras opciones, como hacer el juego en 2D, pero se descartó para aprender sobre Shaders y posicionamiento en 3D.

### Posibles ampliaciones (Futuro):
* **Deck Builder:** Permitir al usuario crear su propio mazo antes de la partida (guardando datos en JSON).
* **Multijugador:** Implementar juego en red local (LAN) para jugar contra otra persona.
* **Cartas de Hechizo:** Añadir cartas que no ocupen espacio en el tablero y tengan efectos instantáneos.
* **Cartas con Efectos:** Añadir posibles efectos a las cartas (congelar, quemar, curar).

## 6. Conclusiones

El desarrollo de **ArcanaClash** ha sido un reto integrador. He conseguido unificar la lógica estricta de la programación en C# (listas, bucles, matrices, POO) con el componente creativo del desarrollo de videojuegos (Shaders, partículas, UI).

El resultado es un juego funcional, visualmente atractivo y con un código estructurado que permite su mantenimiento y ampliación, cumpliendo con los objetivos marcados al inicio del Proyecto Intermodular.