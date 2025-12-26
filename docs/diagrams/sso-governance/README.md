# Modelo de Gobernanza, Roles y Accesos del Sistema de Autenticación Única (SSO)

El presente apartado describe el modelo de gobernanza, administración de roles y control de accesos del Sistema de Autenticación Única (SSO), así como su relación con los sistemas integrados.  
La explicación se apoya en tres diagramas conceptuales elaborados en lenguaje Mermaid (`.mmd`), los cuales permiten comprender de manera progresiva el funcionamiento del SSO desde su arranque hasta la operación diaria.

## Diagrama completo
```mermaid
flowchart LR

    %% ===============================
    %% CATEGORÍA 0 – USUARIO TÉCNICO
    %% ===============================
    subgraph CAT0[CAT 0]
        direction TB
        H0[Categoría 0 - Usuario Técnico del SSO]

        T0[Usuario Técnico del SSO]
        T1[Configuración inicial del SSO]
        T2[Designar Super Administradores del SSO]
        T3[Recuperación administrativa del SSO]
        T4[Uso restringido y excepcional]

        H0 --> T0
        T0 --> T1
        T0 --> T2
        T0 --> T3
        T0 --> T4
    end

    %% ===============================
    %% CATEGORÍA 1 – SUPER ADMIN SSO
    %% ===============================
    subgraph CAT1[CAT 1]
        direction TB
        H1[Categoría 1 - Super Administradores del SSO]

        SA[Super Admin SSO]

        A1[Administrar sistemas registrados]
        A2[Administrar usuarios globales]
        A3[Administrar roles globales]
        A4[Supervisar operación del SSO]
        A5[Designar administradores de sistemas]

        H1 --> SA
        SA --> A1
        SA --> A2
        SA --> A3
        SA --> A4
        SA --> A5
    end

    %% ===============================
    %% CATEGORÍA 2 – ADMIN SISTEMA
    %% ===============================
    subgraph CAT2[CAT 2]
        direction TB
        H2[Categoría 2 - Administrador de Sistema]

        AS[Administrador de Sistema]

        B1[Administrar usuarios de su sistema]
        B2[Asignar roles de su sistema]
        B3[Acceso parcial al portal SSO]
        B4[Operación del sistema asignado]

        H2 --> AS
        AS --> B1
        AS --> B2
        AS --> B3
        AS --> B4
    end

    %% ===============================
    %% CATEGORÍA 3 – USUARIO FINAL
    %% ===============================
    subgraph CAT3[CAT 3]
        direction TB
        H3[Categoría 3 - Usuario de Sistemas]

        U[Usuario]

        C1[Autenticación mediante SSO]
        C2[Acceso a sistemas autorizados]
        C3[Uso funcional del sistema]

        H3 --> U
        U --> C1
        U --> C2
        U --> C3
    end

    %% ===============================
    %% RELACIONES DE DESIGNACIÓN
    %% ===============================
    T2 -.->|designa| SA
    SA -.->|designa| AS
    AS -.->|gestiona| U

    %% ===============================
    %% ESTILOS
    %% ===============================
    classDef header fill:#ffffff,stroke:#ffffff,color:#2d3436,font-weight:bold;
    class H0,H1,H2,H3 header;

    classDef tecnico fill:#dfe6e9,stroke:#2d3436,stroke-width:2px,font-weight:bold;
    classDef super fill:#ffcccc,stroke:#c0392b,stroke-width:2px;
    classDef admin fill:#ffe9b3,stroke:#e67e22,stroke-width:2px;
    classDef user fill:#d5ffd9,stroke:#27ae60,stroke-width:2px;

    class T0 tecnico
    class SA super
    class AS admin
    class U user
```

---

## 1. Gobernanza y Arranque del Sistema de Autenticación Única

**Referencia:** *Diagrama 1 – Gobernanza del SSO (Arranque y Administración)*
```mermaid
flowchart TB

    %% ===============================
    %% CATEGORÍA 0 – USUARIO TÉCNICO
    %% ===============================
    subgraph CAT0[CAT 0]
        direction TB
        H0[Usuario Técnico del SSO]

        T0[Configuración inicial del SSO]
        T1[Creación de Super Administradores del SSO]
        T2[Recuperación administrativa del SSO]
        T3[Uso restringido y excepcional]

        H0 --> T0
        H0 --> T1
        H0 --> T2
        H0 --> T3
    end

    %% ===============================
    %% CATEGORÍA 1 – SUPER ADMIN SSO
    %% ===============================
    subgraph CAT1[CAT 1]
        direction TB
        H1[Super Administradores del SSO]

        A1[Administrar sistemas registrados]
        A2[Administrar usuarios globales]
        A3[Administrar roles globales]
        A4[Supervisar operación del SSO]
        A5[Designar administradores de sistemas]

        H1 --> A1
        H1 --> A2
        H1 --> A3
        H1 --> A4
        H1 --> A5
    end

    %% ===============================
    %% RELACIÓN DE DESIGNACIÓN
    %% ===============================
    T1 -.->|designa| H1

    %% ===============================
    %% ESTILOS
    %% ===============================
    classDef header fill:#ffffff,stroke:#ffffff,color:#2d3436,font-weight:bold;
    class H0,H1 header;

```

El primer diagrama describe el proceso de **arranque institucional del SSO** y los roles responsables de su administración a nivel global.

### Usuario Técnico del SSO (Categoría 0)

El Usuario Técnico del SSO es una cuenta especial, no asociada a una persona natural, cuya finalidad es:

- Realizar la configuración inicial del SSO.
- Crear y designar a los Super Administradores del SSO.
- Permitir la recuperación administrativa del sistema ante contingencias.
- Garantizar la continuidad operativa del SSO.

Este usuario tiene un **uso restringido y excepcional**, y no forma parte de la operación cotidiana del sistema.

### Super Administradores del SSO (Categoría 1)

Los Super Administradores del SSO representan a **personas naturales debidamente identificadas**, responsables de la administración integral del SSO una vez que el sistema se encuentra operativo.

Entre sus funciones principales se encuentran:

- Administrar los sistemas registrados en el SSO.
- Gestionar usuarios globales.
- Gestionar roles globales.
- Supervisar la operación general del SSO.
- Designar a los administradores de los sistemas integrados.

El diagrama enfatiza que el Usuario Técnico **designa** a los Super Administradores del SSO, sin implicar que se trate de la misma persona.

---

## 2. Administración y Uso de los Sistemas Integrados

**Referencia:** *Diagrama 2 – Administración y Uso de los Sistemas Integrados*
```mermaid
flowchart TB

    %% ===============================
    %% CATEGORÍA 1 – SUPER ADMIN SSO
    %% ===============================
    subgraph CAT1[CAT 1]
        direction TB
        H1[Super Administrador del SSO]

        S1[Registrar sistemas en el SSO]
        S2[Designar administradores de sistemas]

        H1 --> S1
        H1 --> S2
    end

    %% ===============================
    %% CATEGORÍA 2 – ADMIN SISTEMA
    %% ===============================
    subgraph CAT2[CAT 2]
        direction TB
        H2[Administrador de Sistema]

        A1[Administrar usuarios de su sistema]
        A2[Asignar roles del sistema]
        A3[Acceso parcial al portal SSO]
        A4[Operación del sistema]

        H2 --> A1
        H2 --> A2
        H2 --> A3
        H2 --> A4
    end

    %% ===============================
    %% CATEGORÍA 3 – USUARIO FINAL
    %% ===============================
    subgraph CAT3[CAT 3]
        direction TB
        H3[Usuario del Sistema]

        U1[Autenticarse mediante SSO]
        U2[Acceder a sistemas autorizados]
        U3[Uso funcional del sistema]

        H3 --> U1
        H3 --> U2
        H3 --> U3
    end

    %% ===============================
    %% RELACIONES DE GESTIÓN
    %% ===============================
    S2 -.->|designa| H2
    H2 -.->|gestiona| H3

    %% ===============================
    %% ESTILOS
    %% ===============================
    classDef header fill:#ffffff,stroke:#ffffff,color:#2d3436,font-weight:bold;
    class H1,H2,H3 header;
```

El segundo diagrama explica cómo el SSO se relaciona con los sistemas integrados (como PAT, SISR u otros), una vez que la plataforma ya se encuentra en funcionamiento.

### Administrador de Sistema (Categoría 2)

El Administrador de Sistema es un usuario responsable de la gestión de un sistema específico integrado al SSO.  
Sus funciones incluyen:

- Administrar usuarios de su propio sistema.
- Asignar roles definidos por dicho sistema.
- Acceder al portal del SSO con permisos parciales.
- Supervisar la operación funcional del sistema bajo su responsabilidad.

Este rol es **designado por un Super Administrador del SSO**, lo cual representa una relación de delegación administrativa, no de jerarquía personal.

### Usuario del Sistema (Categoría 3)

El Usuario del Sistema es el usuario final que consume los servicios de uno o más sistemas integrados.

Sus capacidades se limitan a:

- Autenticarse mediante el SSO.
- Acceder únicamente a los sistemas para los cuales ha sido autorizado.
- Utilizar las funcionalidades propias del sistema según los roles asignados.

El diagrama deja explícito que el Administrador de Sistema **gestiona** usuarios, pero no implica que una misma persona deba cumplir ambos roles.

---

## 3. Separación entre Roles Globales y Roles por Sistema

**Referencia:** *Diagrama 3 – Separación de Roles Globales y Roles por Sistema*
```mermaid
flowchart TB

    %% ===============================
    %% IDENTIDAD GLOBAL
    %% ===============================
    subgraph ID[Identidad en el SSO]
        direction TB
        U[Usuario Global]
        R0[Rol Global SSO]

        U --> R0
    end

    %% ===============================
    %% ROLES GLOBALES
    %% ===============================
    subgraph RG[Roles Globales del SSO]
        direction TB
        G1[SSO_SUPER_ADMIN]
        G2[SSO_ADMIN]
        G3[SSO_AUDITOR]

        R0 --> G1
        R0 --> G2
        R0 --> G3
    end

    %% ===============================
    %% SISTEMAS INTEGRADOS
    %% ===============================
    subgraph SYS[Sistemas Integrados]
        direction TB
        S1[PAT]
        S2[SISR]
        S3[Otros Sistemas]
    end

    %% ===============================
    %% ROLES POR SISTEMA
    %% ===============================
    subgraph RS[Roles por Sistema]
        direction TB
        P1[PAT_ADMIN]
        P2[PAT_USER]

        S1 --> P1
        S1 --> P2
    end

    %% ===============================
    %% ASIGNACIÓN DE ROLES
    %% ===============================
    U -.->|asignado a| S1
    U -.->|asignado a| S2

    U -.->|tiene| G1
    U -.->|tiene| P1

    %% ===============================
    %% ESTILOS
    %% ===============================
    classDef identity fill:#dfe6e9,stroke:#2d3436,stroke-width:2px;
    classDef global fill:#ffcccc,stroke:#c0392b,stroke-width:2px;
    classDef system fill:#ffe9b3,stroke:#e67e22,stroke-width:2px;

    class U identity
    class G1,G2,G3 global
    class P1,P2 system
```

El tercer diagrama clarifica uno de los principios fundamentales del modelo: la **separación de responsabilidades entre el SSO y los sistemas integrados**.

### Usuario Global

El Usuario Global representa la identidad única registrada en el SSO.  
Esta identidad:

- Existe una sola vez en la plataforma.
- Puede ser asignada a uno o más sistemas.
- Puede tener roles globales y roles por sistema de forma independiente.

### Roles Globales del SSO

Los Roles Globales controlan las capacidades del usuario **dentro del SSO**, tales como:

- Administración del SSO.
- Auditoría.
- Gestión de usuarios y sistemas.

Ejemplos de roles globales:
- `SSO_SUPER_ADMIN`
- `SSO_ADMIN`
- `SSO_AUDITOR`

Estos roles **no otorgan permisos funcionales dentro de los sistemas integrados**.

### Roles por Sistema

Cada sistema integrado define y administra sus propios roles, los cuales regulan las funcionalidades internas del sistema.

Ejemplos:
- `PAT_ADMIN`
- `PAT_USER`

El SSO no interpreta ni gobierna estos roles; únicamente los transporta como parte del contexto de autenticación.

---

## 4. Principio Rector del Modelo

> El Sistema de Autenticación Única es responsable de la gestión de identidades y roles globales, mientras que los sistemas integrados son responsables de la definición y aplicación de sus roles funcionales.

Este principio garantiza:

- Separación clara de responsabilidades.
- Escalabilidad del ecosistema de sistemas.
- Facilidad de auditoría.
- Continuidad institucional ante cambios de personal.

---
## 5. Diagrama consolidado


## 6. Consideraciones Finales

El uso de los tres diagramas permite una comprensión progresiva del modelo:

1. **Diagrama 1:** explica quién gobierna el SSO y cómo se inicia.
2. **Diagrama 2:** muestra cómo se administran y utilizan los sistemas integrados.
3. **Diagrama 3:** delimita claramente los roles globales y los roles por sistema.
4. 

Este enfoque modular facilita la inclusión del modelo en documentos normativos, manuales técnicos y procesos de auditoría institucional.

