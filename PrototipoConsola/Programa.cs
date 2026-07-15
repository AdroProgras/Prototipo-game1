using System;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        TableroLogica juego = new TableroLogica();
        
        // SE AGREGÓ: Selector de dificultad al arrancar el programa
        Console.Clear();
        Console.WriteLine("=== SELECCIONÁ LA DIFICULTAD ===");
        Console.WriteLine("1. Modo Fácil (Solo movimientos horizontales)");
        Console.WriteLine("2. Modo Difícil (Movimientos horizontales y verticales por zonas)");
        Console.Write("Opción (1 o 2): ");
        
        string opcion = Console.ReadLine().Trim();
        bool modoDificil = (opcion == "2");

        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== PROTOTIPO: INVERSIÓN ASIMÉTRICA ===");
            Console.WriteLine($"Modo actual: {(modoDificil ? "DIFÍCIL 🔴" : "FÁCIL 🟢")}");
            Console.WriteLine("Estados: 0 = BLANCO, 1 = NEGRO (Solo podés tocar los 1s)\n");

            juego.DibujarTableroConsola();

            Console.WriteLine("\nEscribí la fila (A-F) y el botón (1-6) separados por espacio.");
            Console.WriteLine("Ejemplo: B 3 (Escribí 'salir' para terminar)");
            Console.Write("Tu jugada: ");

            string entrada = Console.ReadLine().Trim().ToUpper();
            if (entrada == "SALIR") break;

            string[] partes = entrada.Split(' ');
            if (partes.Length == 2)
            {
                if (partes[0].Length == 1 && int.TryParse(partes[1], out int botonInput))
                {
                    char filaInput = partes[0][0];
                    
                    // Se pasa el parámetro de dificultad a la función core
                    bool exito = juego.PresionarBotónJugador(filaInput, botonInput - 1, modoDificil);
                    if (!exito)
                    {
                        Console.WriteLine("\n[ERROR] ¡Movimiento inválido! Recordá que solo podés tocar botones NEGROS (1) dentro del rango.");
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
                            Console.WriteLine("\nPresioná cualquier tecla para salir...");
                            Console.ReadKey();
                            break;
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

    public TableroLogica()
    {
        InicializarTablero();
    }

    private void InicializarTablero()
    {
        // Tablero inicializado con algunos 1s estratégicos para probar las conexiones
        matrizTablero['A'] = new int[] { 0, 1, 0 };
        matrizTablero['B'] = new int[] { 1, 0, 1, 0, 1 };
        matrizTablero['C'] = new int[] { 0, 0, 1, 1, 0, 0 };
        matrizTablero['D'] = new int[] { 0, 1, 0, 0, 1, 0 };
        matrizTablero['E'] = new int[] { 1, 0, 0, 0, 1 };
        matrizTablero['F'] = new int[] { 0, 1, 0 };
    }

    // SE MODIFICÓ: Ahora recibe el parámetro 'esDificil' para evaluar la vecindad vertical reciprocada
    public bool PresionarBotónJugador(char fila, int indiceBoton, bool esDificil)
    {
        if (!matrizTablero.ContainsKey(fila) || indiceBoton < 0 || indiceBoton >= matrizTablero[fila].Length)
            return false;

        if (matrizTablero[fila][indiceBoton] == 0)
            return false;

        // 1. Invertir el botón que el jugador presionó
        InvertirBoton(fila, indiceBoton);

        // 2. Vecinos Horizontales (Aplica siempre en Fácil y Difícil)
        InvertirBoton(fila, indiceBoton - 1);
        InvertirBoton(fila, indiceBoton + 1);

        // 3. Vecinos Verticales (Mapeo recíproco estricto del GDD y su lista, solo en Modo Difícil)
        if (esDificil)
        {
            CalcularVecinosVerticales(fila, indiceBoton);
        }

        return true;
    }

    // SE AGREGÓ: Función dedicada a procesar su lista de conexiones recíprocas sin redundancia
    private void CalcularVecinosVerticales(char fila, int indice)
    {
        int botonNum = indice + 1; // Pasamos a base 1 para que calce idéntico a su lista

        // ZONA SUPERIOR (Filas A y B)
        if (fila == 'A')
        {
            if (botonNum == 1) InvertirBoton('B', 1); // A1 conecta con B2 (índice 1)
            if (botonNum == 2) InvertirBoton('B', 2); // A2 conecta con B3 (índice 2)
            if (botonNum == 3) InvertirBoton('B', 3); // A3 conecta con B4 (índice 3)
        }
        else if (fila == 'B')
        {
            if (botonNum == 2) InvertirBoton('A', 0); // B2 conecta con A1 (índice 0)
            if (botonNum == 3) InvertirBoton('A', 1); // B3 conecta con A2 (índice 1)
            if (botonNum == 4) InvertirBoton('A', 2); // B4 conecta con A3 (índice 2)
            // Botones 1 y 5 son puntos ciegos (Opción A), no tienen vecinos arriba
        }

        // ZONA CENTRAL (Filas C y D conectadas 1:1 perfectamente)
        else if (fila == 'C')
        {
            InvertirBoton('D', indice); // C conecta directamente con su espejo abajo en D
        }
        else if (fila == 'D')
        {
            InvertirBoton('C', indice); // D conecta directamente con su espejo arriba en C
        }

        // ZONA INFERIOR (Filas E y F - Espejo de la zona superior)
        else if (fila == 'F')
        {
            if (botonNum == 1) InvertirBoton('E', 1); // F1 conecta con E2 (índice 1)
            if (botonNum == 2) InvertirBoton('E', 2); // F2 conecta con E3 (índice 2)
            if (botonNum == 3) InvertirBoton('E', 3); // F3 conecta con E4 (índice 3)
        }
        else if (fila == 'E')
        {
            if (botonNum == 2) InvertirBoton('F', 0); // E2 conecta con F1 (índice 0)
            if (botonNum == 3) InvertirBoton('F', 1); // E3 conecta con F2 (índice 1)
            if (botonNum == 4) InvertirBoton('F', 2); // E4 conecta con F3 (índice 2)
            // Botones 1 y 5 son puntos ciegos, no tienen vecinos abajo
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

    public bool EsVictoria()
    {
        foreach (var fila in matrizTablero)
        {
            int[] arreglo = fila.Value;
            for (int i = 0; i < arreglo.Length; i++)
            {
                if (arreglo[i] == 1)
                    return false;
            }
        }
        return true;
    }

    public void DibujarTableroConsola()
    {
        foreach (var fila in matrizTablero)
        {
            if (fila.Key == 'A' || fila.Key == 'F') { Console.Write("    "); }
            if (fila.Key == 'B' || fila.Key == 'E') { Console.Write("  "); }
            if (fila.Key == 'C' || fila.Key == 'D') { Console.Write(""); }

            Console.Write(fila.Key + " -> [ ");
            int[] arreglo = fila.Value;
            for (int i = 0; i < arreglo.Length; i++)
            {
                Console.Write(arreglo[i] + " ");
            }
            Console.WriteLine("]");
        }
    }
}
