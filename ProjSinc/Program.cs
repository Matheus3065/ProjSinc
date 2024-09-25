using System;
using System.IO;
using System.Linq;
using System.Timers;
using Timer = System.Timers.Timer;


class Program
{
    static string caminhoFonte;
    static string caminhoReplica;
    static string caminhoLog;
    static double intervaloSincronizacao; // em milissegundos
    static Timer timer;

    static void Main(string[] args)
    {
        // Verificar se a quantidade de argumentos é correta
        if (args.Length < 4)
        {
            Console.WriteLine("Uso: FolderSyncApp.exe <caminhoFonte> <caminhoReplica> <intervaloEmSegundos> <caminhoLog>");
            return;
        }

        // Atribuir os valores dos argumentos
        caminhoFonte = args[0];
        caminhoReplica = args[1];
        intervaloSincronizacao = double.Parse(args[2]) * 1000; // Converter para milissegundos
        caminhoLog = args[3];

        // Exibir os argumentos para diagnóstico
        Console.WriteLine("Argumentos fornecidos:");
        Console.WriteLine($"Pasta Fonte: {caminhoFonte}");
        Console.WriteLine($"Pasta Réplica: {caminhoReplica}");
        Console.WriteLine($"Intervalo de Sincronização: {intervaloSincronizacao / 1000} segundos");
        Console.WriteLine($"Arquivo de Log: {caminhoLog}");

        // Verificar se as pastas fornecidas existem
        if (!Directory.Exists(caminhoFonte))
        {
            Console.WriteLine($"Erro: A pasta fonte '{caminhoFonte}' não existe.");
            return;
        }

        if (!Directory.Exists(caminhoReplica))
        {
            Console.WriteLine($"A pasta réplica '{caminhoReplica}' não existe. Criando a pasta...");
            Directory.CreateDirectory(caminhoReplica);
        }

        // Registrar que a sincronização foi iniciada
        RegistrarMensagem("Sincronização iniciada.");

        // Executar a sincronização imediatamente ao iniciar
        SincronizarPastas(null, null);

        // Configurar o Timer para sincronizar periodicamente
        timer = new Timer(intervaloSincronizacao);
        timer.Elapsed += SincronizarPastas;
        timer.Start();

        Console.WriteLine("Pressione [Enter] para encerrar o programa.");
        Console.ReadLine();
        timer.Stop();
    }

    // Método que sincroniza as pastas
    static void SincronizarPastas(object sender, ElapsedEventArgs e)
    {
        try
        {
            // Sincronizar arquivos e diretórios
            CopiarArquivosRecursivamente(caminhoFonte, caminhoReplica);

            // Remover arquivos/diretórios extras que não estão mais na pasta fonte
            RemoverArquivosExtras(caminhoReplica, caminhoFonte);

            RegistrarMensagem("Sincronização concluída.");
        }
        catch (Exception ex)
        {
            RegistrarMensagem($"Erro durante a sincronização: {ex.Message}");
        }
    }

    // Método que copia arquivos de forma recursiva da fonte para a réplica
    static void CopiarArquivosRecursivamente(string dirFonte, string dirAlvo)
    {
        Directory.CreateDirectory(dirAlvo);

        foreach (var arquivo in Directory.GetFiles(dirFonte))
        {
            var caminhoAlvo = Path.Combine(dirAlvo, Path.GetFileName(arquivo));
            if (!File.Exists(caminhoAlvo) || File.GetLastWriteTime(arquivo) > File.GetLastWriteTime(caminhoAlvo))
            {
                File.Copy(arquivo, caminhoAlvo, true);
                RegistrarMensagem($"Arquivo copiado: {arquivo} -> {caminhoAlvo}");
            }
        }

        foreach (var diretorio in Directory.GetDirectories(dirFonte))
        {
            var subDirAlvo = Path.Combine(dirAlvo, Path.GetFileName(diretorio));
            CopiarArquivosRecursivamente(diretorio, subDirAlvo);
        }
    }

    // Método que remove arquivos e diretórios extras da réplica que não estão mais na fonte
    static void RemoverArquivosExtras(string dirAlvo, string dirFonte)
    {
        foreach (var arquivo in Directory.GetFiles(dirAlvo))
        {
            var caminhoFonte = Path.Combine(dirFonte, Path.GetFileName(arquivo));
            if (!File.Exists(caminhoFonte))
            {
                File.Delete(arquivo);
                RegistrarMensagem($"Arquivo excluído: {arquivo}");
            }
        }

        foreach (var diretorio in Directory.GetDirectories(dirAlvo))
        {
            var subDirFonte = Path.Combine(dirFonte, Path.GetFileName(diretorio));
            if (!Directory.Exists(subDirFonte))
            {
                Directory.Delete(diretorio, true);
                RegistrarMensagem($"Diretório excluído: {diretorio}");
            }
            else
            {
                RemoverArquivosExtras(diretorio, subDirFonte);
            }
        }
    }

    // Método para registrar mensagens de log
    static void RegistrarMensagem(string mensagem)
    {
        var mensagemLog = $"{DateTime.Now}: {mensagem}";
        Console.WriteLine(mensagemLog);
        File.AppendAllText(caminhoLog, mensagemLog + Environment.NewLine);
    }
}

