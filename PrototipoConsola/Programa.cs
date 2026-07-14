using System;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        TableroLogica juego = new TableroLogica();

        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== PROTOTIPO: INVERSIÓN ASIMÉTRICA ===");
            Console.WriteLine("Estados: 0 = BLANCO, 1 = NEGRO (Solo podés tocar los 1s)\n");

            juego.DibujarTableroConsola();

            Console.WriteLine("\nEscribí la fila (A-F) y el botón (1-6) separados por espacio.");
            Console.WriteLine("Ejemplo: B 3 (Escribí 'salir' para terminar)");
            Console.Write("Tu jugada: ");

            string entrada = Console.ReadLine().Trim().ToUpper();
            if (entrada == "SALIR") break;

            string[] partes = entrada.Split(' ');
            if (partes.Length == 2 && partes[0].Length == 1)
            {
                char filaInput = partes[0][0];
                if (int.TryParse(partes[1], out int botonInput))
                {
                    bool exito = juego.PresionarBotónJugador(filaInput, botonInput - 1);
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
    // SE CORRIGIÓ: Se agregaron los tipos genéricos <char, int[]> que se borraron en el correo
    private Dictionary<char, int[]> matrizTablero = new Dictionary<char, int[]>();

    public TableroLogica()
    {
        InicializarTablero();
    }

    private void InicializarTablero()
    {
        // Se definen los estados iniciales (algunos 1s para poder jugar y probar)
        matrizTablero['A'] = new int[] { 0, 0, 0 };
        matrizTablero['B'] = new int[] { 0, 0, 1, 0, 0 };
        matrizTablero['C'] = new int[] { 0, 0, 0, 1, 0, 0 };
        matrizTablero['D'] = new int[] { 0, 1, 0, 0, 0, 0 };
        matrizTablero['E'] = new int[] { 0, 0, 0, 0, 0 };
        matrizTablero['F'] = new int[] { 0, 0, 0 };
    }

    public bool PresionarBotónJugador(char fila, int indiceBoton)
    {
        if (!matrizTablero.ContainsKey(fila) || indiceBoton < 0 || indiceBoton >= matrizTablero[fila].Length)
            return false;

        // REGLA DEL GDD: Solo se pueden tocar botones NEGROS (1)
        if (matrizTablero[fila][indiceBoton] == 0)
            return false;

        // Efecto dominó Modo Fácil (horizontales de la misma fila)
        InvertirBoton(fila, indiceBoton);
        InvertirBoton(fila, indiceBoton - 1);
        InvertirBoton(fila, indiceBoton + 1);

        return true;
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
            // SE CORRIGIÓ: Centrado y desfase visual según las reglas del GDD
            if (fila.Key == 'A' || fila.Key == 'F') { Console.Write("    "); } // 3 botones
            if (fila.Key == 'B' || fila.Key == 'E') { Console.Write("  "); }   // 5 botones
            if (fila.Key == 'C' || fila.Key == 'D') { Console.Write(""); }     // 6 botones

            Console.Write(fila.Key + " -> [ ");
            int[] arreglo = fila.Value;
            for (int i = 0; i < arreglo.Length; i++)
            {
                Console.Write(arreglo[i] + " ");
            }
            Console.WriteLine("]");
        }
    }
