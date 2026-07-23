using System;
using System.Collections.Generic;
using System.Text;

[System.Serializable]
public struct NivelConfig
{
    public int NumeroNivel;
    public bool EsDificil;
    public string HashInicial;      // El string de 28 unos y ceros fijo
    public int MovimientosMaximos;  // Límite de toques del jugador
    public string RutaSolucion;     // La solución óptima ("A1 B3") por si las pistas
}

class Program
{
    static void Main(string[] args)
    {
        while (true)
        {
            TableroLogica juego = new TableroLogica();
            
            Console.Clear();
            Console.WriteLine("=== PROTOTIPO: MOTOR GENERADOR BFS ===");
            Console.WriteLine("1. Modo Fácil (Solo movimientos horizontales)");
            Console.WriteLine("2. Modo Difícil (Movimientos horizontales y verticales por zonas)");
            Console.Write("Opción (1 o 2) o 'salir': ");
            
            string opcion = Console.ReadLine().Trim();
            if (opcion.ToLower() == "salir") break;
            bool modoDificil = (opcion == "2");

            Console.WriteLine("\n[CONFIGURACIÓN DE DIFICULTAD INTERACTIVA]");
            List<int> configuracionToques = new List<int>();

            if (!modoDificil)
            {
                char[] filas = { 'A', 'B', 'C', 'D', 'E', 'F' };
                Console.WriteLine("Defina cuántos toques quiere aplicar en cada FILA (Máximo recomendado: 2 o 3):");
                foreach (char f in filas)
                {
                    Console.Write($"Toques para Fila {f}: ");
                    if (!int.TryParse(Console.ReadLine(), out int t) || t < 0) t = 0;
                    configuracionToques.Add(t);
                }
            }
            else
            {
                Console.WriteLine("Defina cuántos toques quiere aplicar en cada SECCIÓN:");
                string[] secciones = { "SUPERIOR (A-B)", "CENTRAL (C-D)", "INFERIOR (E-F)" };
                foreach (string s in secciones)
                {
                    Console.Write($"Toques para Sección {s}: ");
                    if (!int.TryParse(Console.ReadLine(), out int t) || t < 0) t = 0;
                    configuracionToques.Add(t);
                }
            }

            Console.WriteLine("\n[MOTOR] Ejecutando ingeniería inversa y validación BFS por zonas...");
            
            // Llamada limpia al nuevo motor por arreglos
            string secuenciaSolucion = juego.GenerarNivelPerfectoPorArreglo(configuracionToques, modoDificil);

            if (string.IsNullOrEmpty(secuenciaSolucion))
            {
                Console.WriteLine("\n[ERROR] No se pudo encontrar un mapa con esa profundidad exacta. Intentá de nuevo.");
                Console.ReadKey();
                continue;
            }

            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== PROTOTIPO: INVERSIÓN ASIMÉTRICA ===");
                Console.WriteLine($"Modo actual: {(modoDificil ? "DIFÍCIL 🔴" : "FÁCIL 🟢")}");
                Console.WriteLine($"[TESTING] Secuencia óptima para ganar: {secuenciaSolucion}");
                Console.WriteLine("Estados: 0 = BLANCO, 1 = NEGRO (Solo podés tocar los 1s)\n");

                juego.DibujarTableroConsola();

                Console.WriteLine("\nEscribí la fila (A-F) and el botón (1-6) separados por espacio (o 'rendirse').");
                Console.Write("Tu jugada: ");

                string entrada = Console.ReadLine().Trim().ToUpper();
                if (entrada == "RENDIRSE") break;

                string[] partes = entrada.Split(' ');
                if (partes.Length == 2)
                {
                    char filaInput = partes[0][0];
                    if (int.TryParse(partes[1], out int botonInput))
                    {
                        bool exito = juego.PresionarBotónJugador(filaInput, botonInput - 1, modoDificil);
                        if (!exito)
                        {
                            Console.WriteLine("\n[ERROR] Movimiento inválido. Solo podés tocar botones NEGROS (1).");
                            Console.ReadKey();
                        }
                        else
                        {
                            if (juego.EsVictoria())
                            {
                                Console.Clear();
                                Console.WriteLine("=================================");
                                Console.WriteLine(" ¡VICTORIA! ¡TABLERO LIMPIO! ");
                                Console.WriteLine("=================================");
                                juego.DibujarTableroConsola();
                                Console.WriteLine("\nPresioná cualquier tecla para continuar...");
                                Console.ReadKey();
                                break;
                            }
                        }
                    }
                }
            }
            
        }
    }
}

public class TableroLogica
{
    private Dictionary<char, int[]> matrizTablero = new Dictionary<char, int[]>();
    private List<char> ordenFilas = new List<char> { 'A', 'B', 'C', 'D', 'E', 'F' };

    public TableroLogica()
    {
        InicializarTableroLimpio();
    }

    private void InicializarTableroLimpio()
    {
        matrizTablero['A'] = new int[] { 0, 0, 0 };
        matrizTablero['B'] = new int[] { 0, 0, 0, 0, 0 };
        matrizTablero['C'] = new int[] { 0, 0, 0, 0, 0, 0 };
        matrizTablero['D'] = new int[] { 0, 0, 0, 0, 0, 0 };
        matrizTablero['E'] = new int[] { 0, 0, 0, 0, 0 };
        matrizTablero['F'] = new int[] { 0, 0, 0 };
    }

    // FASE 1 Y 2 UNIFICADAS: La función monstrua
    // PASO 4: El nuevo motor unificado e inteligente por barajado de estados
    public string GenerarNivelPerfectoPorArreglo(List<int> toquesPorZona, bool esDificil)
    {
        string solucionTotal = "";
        
        if (!esDificil)
        {
            // === MODO FÁCIL: PROCESA LAS 6 FILAS INDEPENDIENTES ===
            char[] filas = { 'A', 'B', 'C', 'D', 'E', 'F' };
            int[] largosFilas = { 3, 5, 6, 6, 5, 3 };
            int[] limitesInicio = { 0, 3, 8, 14, 20, 25 }; // Índices de inicio reales en el string

            // Plantilla base de 28 ceros para ir guardando el desorden de cada fila juntas
            char[] hashAcumuladoFacil = new string('0', 28).ToCharArray();

            for (int fIdx = 0; fIdx < filas.Length; fIdx++)
            {
                char fila = filas[fIdx];
                int toquesRequeridos = toquesPorZona[fIdx];
                List<char> zonaFila = new List<char> { fila };
                
                if (toquesRequeridos == 0) continue;

                List<int> combinacionesBarajadas = ObtenerCombinacionesBarajadas(largosFilas[fIdx]);
                bool filaResuelta = false;

                foreach (int numDecimal in combinacionesBarajadas)
                {
                    string hashCandidato = GenerarHashInyectado(numDecimal, false, zonaFila);
                    string objetivoFila = new string('0', 28);

                    var bsfFila = ResolverPorBFS(hashCandidato, false, zonaFila, objetivoFila);

                    int pasosRealesFila = 0;
                    if (!string.IsNullOrEmpty(bsfFila.RutaSolucion) && bsfFila.RutaSolucion != "Ya resuelto")
                    {
                        pasosRealesFila = bsfFila.RutaSolucion.Split(' ').Length;
                    }

                    if (pasosRealesFila == toquesRequeridos)
                    {
                        solucionTotal += bsfFila.RutaSolucion + " ";
                        
                        // CORRECCIÓN: Copiamos únicamente los bits de esta fila dentro del hash acumulado
                        char[] candidatoChars = hashCandidato.ToCharArray();
                        int inicio = limitesInicio[fIdx];
                        int largo = largosFilas[fIdx];
                        for (int i = 0; i < largo; i++)
                        {
                            hashAcumuladoFacil[inicio + i] = candidatoChars[inicio + i];
                        }

                        filaResuelta = true;
                        break; 
                    }
                }

                if (!filaResuelta)
                {
                    return ""; 
                }
            }

            // AL PURO FINAL: Cargamos el tablero unificado con todas las filas juntas
            CargarTableroDesdeHash(new string(hashAcumuladoFacil));
            return solucionTotal.Trim();
        }
        else
        {
            // === MODO DIFÍCIL: PROCESA LAS 3 SECCIONES VERTICALES ===
            List<char>[] zonas = {
                new List<char> { 'A', 'B' }, // Superior (3 + 5 = 8 botones)
                new List<char> { 'C', 'D' }, // Central (6 + 6 = 12 botones)
                new List<char> { 'E', 'F' }  // Inferior (5 + 3 = 8 botones)
            };
            int[] largosZonas = { 8, 12, 8 };

            // Inicializamos el tablero completo en un hash base vacío de 26 ceros
            char[] hashAcumulado = new string('0', 28).ToCharArray();

            for (int zIdx = 0; zIdx < zonas.Length; zIdx++)
            {
                int toquesRequeridos = toquesPorZona[zIdx];
                List<char> zonaActual = zonas[zIdx];

                if (toquesRequeridos == 0) continue;

                List<int> combinacionesBarajadas = ObtenerCombinacionesBarajadas(largosZonas[zIdx]);
                bool zonaResuelta = false;

                // Determinamos los índices globales en el string para limpiar solo esta zona
                int inicioZona = (zIdx == 0) ? 0 : (zIdx == 1) ? 8 : 20;
                int largoZona = largosZonas[zIdx];

                foreach (int numDecimal in combinacionesBarajadas)
                {
                    // Generamos el desorden aislado de esta zona
                    string hashZonal = GenerarHashInyectado(numDecimal, true, zonaActual);
                    char[] candidatosZonales = hashZonal.ToCharArray();

                    // Incrustamos el caos de esta zona encima del hash acumulado del tablero entero
                    char[] testTableroCompleto = (char[])hashAcumulado.Clone();
                    for (int i = 0; i < largoZona; i++)
                    {
                        testTableroCompleto[inicioZona + i] = candidatosZonales[inicioZona + i];
                    }

                    string hashPruebaCompleto = new string(testTableroCompleto);

                    // El objetivo del BFS es dejar en '0' únicamente los botones de esta zona específica
                    char[] maskObjetivo = (char[])hashPruebaCompleto.ToCharArray();
                    for (int i = 0; i < largoZona; i++)
                    {
                        maskObjetivo[inicioZona + i] = '0';
                    }
                    string objetivoZona = new string(maskObjetivo);

                    var bsfZona = ResolverPorBFS(hashPruebaCompleto, true, zonaActual, objetivoZona);

                    int pasosRealesZona = 0;
                    if (!string.IsNullOrEmpty(bsfZona.RutaSolucion) && bsfZona.RutaSolucion != "Ya resuelto")
                    {
                        pasosRealesZona = bsfZona.RutaSolucion.Split(' ').Length;
                    }

                    if (pasosRealesZona == toquesRequeridos)
                    {
                        if (bsfZona.RutaSolucion != "Ya resuelto")
                        {
                            solucionTotal += bsfZona.RutaSolucion + " ";
                        }
                        
                        // Guardamos este bloque caótico validado dentro del hash acumulado
                        for (int i = 0; i < largoZona; i++)
                        {
                            hashAcumulado[inicioZona + i] = candidatosZonales[inicioZona + i];
                        }
                        
                        zonaResuelta = true;
                        break; 
                    }
                }

                if (!zonaResuelta)
                {
                    return ""; 
                }
            }

            // Al final cargamos el tablero unificado definitivo en la matriz para que el jugador juegue
            CargarTableroDesdeHash(new string(hashAcumulado));
            return solucionTotal.Trim();
        }
    }

    private (int PasosMinimos, string RutaSolucion) ResolverPorBFS(string hashInicial, bool esDificil, List<char> filasZona, string objetivoZona)
{
    int intInicial = ConvertirHashAEntero(hashInicial);
    int intObjetivo = ConvertirHashAEntero(objetivoZona);

    if (intInicial == intObjetivo) return (0, "Ya resuelto");

    // 2. Cambiamos <string> por <int> en la cola y en el HashSet
    Queue<(int Estado, int Profundidad, string Ruta)> cola = new Queue<(int, int, string)>();
    HashSet<int> visitados = new HashSet<int>();

    // 3. Metemos el número inicial a la cola y a los visitados
    cola.Enqueue((intInicial, 0, ""));
    visitados.Add(intInicial);

    // --- OPTIMIZACIÓN DE ÍNDICES ---
    // Calculamos en qué parte del string total empieza y termina la zona que estamos analizando
    int inicioFor = 0;
    int finFor = hashInicial.Length;

    if (!esDificil && filasZona.Count == 1)
    {
        char filaActual = filasZona[0];
        // Límites fijos de tus filas en el string de 26 caracteres:
        // A=0..2, B=3..7, C=8..13, D=14..19, E=20..24, F=25..27
        if (filaActual == 'A') { inicioFor = 0;  finFor = 3;  }
        else if (filaActual == 'B') { inicioFor = 3;  finFor = 8;  }
        else if (filaActual == 'C') { inicioFor = 8;  finFor = 14; }
        else if (filaActual == 'D') { inicioFor = 14; finFor = 20; }
        else if (filaActual == 'E') { inicioFor = 20; finFor = 25; }
        else if (filaActual == 'F') { inicioFor = 25; finFor = 28; }
    }
    else if (esDificil)
    {
        // En modo difícil evaluamos por bloques completos de zonas
        char filaInicio = filasZona[0];
        if (filaInicio == 'A' || filaInicio == 'B') { inicioFor = 0;  finFor = 8;  } // Zona Superior (A-B)
        else if (filaInicio == 'C' || filaInicio == 'D') { inicioFor = 8;  finFor = 20; } // Zona Central (C-D)
        else if (filaInicio == 'E' || filaInicio == 'F') { inicioFor = 20; finFor = 28; } // Zona Inferior (E-F)
    }

     while (cola.Count > 0)
    {
        var actual = cola.Dequeue();

        // 1. Validación numérica directa
        if (actual.Estado == intObjetivo)
        {
            return (actual.Profundidad, actual.Ruta.Trim());
        }

        // 2. Traducimos una sola vez para usar sus funciones de strings
        string estadoString = ConvertirEnteroAHash(actual.Estado);

        for (int i = inicioFor; i < finFor; i++)
        {
            if (estadoString[i] == '1')
            {
                var (fila, indiceBoton) = TraducirIndiceStringATablero(i);
                
                CargarTableroDesdeHash(estadoString);
                
                InvertirBoton(fila, indiceBoton);
                InvertirBoton(fila, indiceBoton - 1);
                InvertirBoton(fila, indiceBoton + 1);
                if (esDificil) CalcularVecinosVerticales(fila, indiceBoton);
                
                string nuevoHash = ObtenerHashTablero();
                
                // 3. Convertimos a entero para el HashSet numérico
                int nuevoHashInt = ConvertirHashAEntero(nuevoHash);

                if (!visitados.Contains(nuevoHashInt))
                {
                    visitados.Add(nuevoHashInt);
                    string pasoActual = $"{fila}{indiceBoton + 1}";
                    cola.Enqueue((nuevoHashInt, actual.Profundidad + 1, actual.Ruta + " " + pasoActual));
                }
            }
        }
    }
    return (-1, "");
}

    public bool PresionarBotónJugador(char fila, int indiceBoton, bool esDificil)
    {
        if (!matrizTablero.ContainsKey(fila) || indiceBoton < 0 || indiceBoton >= matrizTablero[fila].Length)
            return false;

        if (matrizTablero[fila][indiceBoton] == 0)
            return false;

        InvertirBoton(fila, indiceBoton);
        InvertirBoton(fila, indiceBoton - 1);
        InvertirBoton(fila, indiceBoton + 1);

        if (esDificil)
        {
            CalcularVecinosVerticales(fila, indiceBoton);
        }

        return true;
    }

    private void CalcularVecinosVerticales(char fila, int indice)
    {
        int botonNum = indice + 1;
        if (fila == 'A')
        {
            if (botonNum == 1) InvertirBoton('B', 1);
            if (botonNum == 2) InvertirBoton('B', 2);
            if (botonNum == 3) InvertirBoton('B', 3);
        }
        else if (fila == 'B')
        {
            if (botonNum == 2) InvertirBoton('A', 0);
            if (botonNum == 3) InvertirBoton('A', 1);
            if (botonNum == 4) InvertirBoton('A', 2);
        }
        else if (fila == 'C')
        {
            InvertirBoton('D', indice);
        }
        else if (fila == 'D')
        {
            InvertirBoton('C', indice);
        }
        else if (fila == 'F')
        {
            if (botonNum == 1) InvertirBoton('E', 1);
            if (botonNum == 2) InvertirBoton('E', 2);
            if (botonNum == 3) InvertirBoton('E', 3);
        }
        else if (fila == 'E')
        {
            if (botonNum == 2) InvertirBoton('F', 0);
            if (botonNum == 3) InvertirBoton('F', 1);
            if (botonNum == 4) InvertirBoton('F', 2);
        }
    }

    private void InvertirBoton(char fila, int indiceBoton)
    {
        if (matrizTablero.ContainsKey(fila) && indiceBoton >= 0 && indiceBoton < matrizTablero[fila].Length)
        {
            int[] arreglo = matrizTablero[fila];
            arreglo[indiceBoton] = (arreglo[indiceBoton] == 0) ? 1 : 0;
        }
    }

    private string ObtenerHashTablero()
    {
        StringBuilder sb = new StringBuilder();
        foreach (char f in ordenFilas)
        {
            foreach (int val in matrizTablero[f]) sb.Append(val);
        }
        return sb.ToString();
    }

    private void CargarTableroDesdeHash(string hash)
    {
        int index = 0;
        foreach (char f in ordenFilas)
        {
            for (int i = 0; i < matrizTablero[f].Length; i++)
            {
                matrizTablero[f][i] = hash[index++] - '0';
            }
        }
    }

    private (char Fila, int Indice) TraducirIndiceStringATablero(int stringIndex)
    {
        int acumulado = 0;
        foreach (char f in ordenFilas)
        {
            int largo = matrizTablero[f].Length;
            if (stringIndex < acumulado + largo)
            {
                return (f, stringIndex - acumulado);
            }
            acumulado += largo;
        }
        return ('A', 0);
    }

    public bool EsVictoria()
    {
        foreach (var fila in matrizTablero)
        {
            foreach (int val in fila.Value) if (val == 1) return false;
        }
        return true;
    }

    public void DibujarTableroConsola()
    {
        foreach (var fila in matrizTablero)
        {
            if (fila.Key == 'A' || fila.Key == 'F') { Console.Write(" "); }
            if (fila.Key == 'B' || fila.Key == 'E') { Console.Write(" "); }
            if (fila.Key == 'C' || fila.Key == 'D') { Console.Write(""); }

            Console.Write(fila.Key + " -> [ ");
            foreach (int val in fila.Value) Console.Write(val + " ");
            Console.WriteLine("]");
        }
    }
     // PASO 2: Generar una lista de todas las combinaciones posibles barajadas al azar
    private List<int> ObtenerCombinacionesBarajadas(int cantidadBotones)
    {
        // Calculamos el total de combinaciones (2 elevado a la cantidad de botones)
        int totalCombinaciones = 1 << cantidadBotones; 
        
        List<int> combinaciones = new List<int>(totalCombinaciones);
        for (int i = 0; i < totalCombinaciones; i++)
        {
            combinaciones.Add(i);
        }

        // Algoritmo Fisher-Yates para desordenar la lista al azar de forma ultra rápida
        Random rand = new Random();
        for (int i = totalCombinaciones - 1; i > 0; i--)
        {
            int j = rand.Next(i + 1);
            int temp = combinaciones[i];
            combinaciones[i] = combinaciones[j];
            combinaciones[j] = temp;
        }

        return combinaciones;
    }
    // PASO 3: Convertir el número entero a binario e inyectarlo en la zona correspondiente del hash
private string GenerarHashInyectado(int combinacionDecimal, bool esDificil, List<char> filasZona)
{
    // Creamos una plantilla del tablero vacío (todo ceros, 26 botones)
    char[] baseTablero = new string('0', 28).ToCharArray();
    
    // Identificamos los índices de inicio y el largo según la zona/fila
    int inicioIndex = 0;
    int largoZona = 0;

    if (!esDificil && filasZona.Count == 1)
    {
        char filaActual = filasZona[0];
        if (filaActual == 'A') { inicioIndex = 0;  largoZona = 3; }
        else if (filaActual == 'B') { inicioIndex = 3;  largoZona = 5; }
        else if (filaActual == 'C') { inicioIndex = 8;  largoZona = 6; }
        else if (filaActual == 'D') { inicioIndex = 14; largoZona = 6; }
        else if (filaActual == 'E') { inicioIndex = 20; largoZona = 5; }
        else if (filaActual == 'F') { inicioIndex = 25; largoZona = 3; }
    }
    else if (esDificil)
    {
        char filaInicio = filasZona[0];
        if (filaInicio == 'A' || filaInicio == 'B') { inicioIndex = 0;  largoZona = 8;  } // Superior (3+5)
        else if (filaInicio == 'C' || filaInicio == 'D') { inicioIndex = 8;  largoZona = 12; } // Central (6+6)
        else if (filaInicio == 'E' || filaInicio == 'F') { inicioIndex = 20; largoZona = 8;  } // Inferior (5+3)
    }

    // Inyectamos el número mapeándolo bit a bit en los índices de la zona
    for (int i = 0; i < largoZona; i++)
    {
        // Revisamos si el bit en la posición 'i' está encendido (es un 1)
        int bit = (combinacionDecimal >> i) & 1;
        baseTablero[inicioIndex + i] = (bit == 1) ? '1' : '0';
    }

    return new string(baseTablero);
}
// PASO 1: Convierte el string de unos y ceros a un entero (Bitmask)
private int ConvertirHashAEntero(string hash)
{
    int bitmask = 0;
    for (int i = 0; i < hash.Length; i++)
    {
        if (hash[i] == '1')
        {
            bitmask |= (1 << i); // Enciende el bit en la posición 'i'
        }
    }
    return bitmask;
}
// PASO 2: Convierte un entero (Bitmask) de vuelta a un string de 28 caracteres
private string ConvertirEnteroAHash(int bitmask)
{
    char[] caracteres = new char[28];
    for (int i = 0; i < 28; i++)
    {
        // Revisa si el bit 'i' está encendido
        caracteres[i] = ((bitmask & (1 << i)) != 0) ? '1' : '0';
    }
    return new string(caracteres);
}
}
