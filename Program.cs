using System;
using System.IO;
using System.Globalization;
using System.Threading;
using Spectre.Console;

/***********************************************************************************
Sistema Bancário

- Consultar saldo
- Fazer depósito
- Fazer Levantamento
- Sair

(o que eu adicionei além do que pedido)
- Sistema de Contas (Criar nova ou usar existente)
- Pagar Contas
- Calendário de Gastos
************************************************************************************/

namespace Variavies_LocaisGLobaisetc
{
    class Sistema_Bancario
    {
        /********************************************************************
        VARIÁVEIS GLOBAIS - Essas variáveis são acessíveis em todo o programa
        ********************************************************************/
        static string nome; // Nome do usuário
        static int idade; // Idade do usuário
        static char euro = '\u20AC'; // importação do símbolo do euro
        static string pastaContas = "contas"; // Pasta onde ficam as contas salvas
        static string arquivoSaldo; // Arquivo do saldo (único para cada pessoa)
        static string arquivoTransacoes; // Arquivo de histórico (único para cada pessoa)
        static string arquivoDados; // Arquivo com nome e idade (único para cada pessoa)
        static float userbalance; // Saldo atual do usuário
        static float deposito; // Valor do depósito temporário
        static float levantamento; // Valor do levantamento temporário

        /********************************************************************
        SISTEMA DE CONTAS - Criar nova ou usar existente
        ********************************************************************/
        
        // Metodo que Pergunta(e cria caso seja escolhida a opção), se quer criar conta nova ou usar uma existente
        static void SistemadeContas()
        {
            Console.Clear();
            Titulo(); //apresenta o titulo 
            
            // Cria a pasta "contas" no disco, se já não existir uma ou se o user escolher criar uma
            if (!Directory.Exists(pastaContas)) //se não existir
            {
                Directory.CreateDirectory(pastaContas); //cria uma pasta
            }
            
            // Pega todas as pastas dentro de "contas" (cada pasta é uma conta)
            
            /* Usa GetDirectories que retorna a quantia de pastas(contas) no disco e armazena numa array
            (fazer isso e bom para ajudar o programa reconhecer as contas existentes no computador e evitar a criação
            de multiplas contas diversas vezes e transformar as pastas em dados temporarios no programa)
            */
            string[] contas = Directory.GetDirectories(pastaContas); //posso traduzir este comando na fala normal como
                                                                     //"diz-me quantas pastas existem e quais são
                                                                     
            int numeroDeContas = contas.Length; //o lenght conta o número de pastas guardadas então da a informação ao
                                                //programa para então saber se e necessário criar obrigatoriamente
                                                //uma nova conta ou não
            
            // Se a array verificar que não há nenhuma conta, o user será obrigado a criar uma conta
            if (numeroDeContas == 0) 
            {
                var panelNovo = new Panel( //criação de paineis para melhor interface grafica com Spectre Console
                    "[yellow]Nenhuma conta encontrada![/]\n\n" +
                    "Vamos criar sua primeira conta.")
                    .Border(BoxBorder.Rounded)
                    .BorderColor(Color.Yellow)
                    .Header("🆕 Bem-vindo!");
                AnsiConsole.Write(Align.Center(panelNovo)); //Apresenta o Painel
                Console.WriteLine();
                Console.ReadKey();
                CriarNovaConta(); //Vai para o metodo de CriarNovaConta se não tiver nenhuma conta criada
                return; //aqui para de executar o if (evitar criar repetições desnecessarias assim que
                        //o user for direcionado para o metodo chamado)
            }
            
            // Se existem contas, mostra menu de escolha com o uso de "SelectionChoices"do spectre console, para mais 
            //interatividade na app
            var panelEscolha = new Panel(
                $"[bold]Encontradas {numeroDeContas} conta(s) no sistema![/]\n\n" +
                "Deseja usar uma conta existente ou criar uma nova?")
                .Border(BoxBorder.Rounded) //estilo da caixinha de texto
                .BorderColor(Color.Green) //cor da caixa
                .Header("💳 Sistema de Contas"); //Header, não sei bem a tradução, mas e algo como um título na caixa
            AnsiConsole.Write(Align.Center(panelEscolha)); //mostra o painel  e alinha ele no centro
            Console.WriteLine();
            
            // Aqui crio o menu interativo com as setinhas usando a sintaxe do Spectre Console (e so altero as opções
            // que quero que apareça)
            var escolha = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[green]Escolha uma opção:[/]")
                    .AddChoices(new[]
                    {
                        "🔑 Usar Conta Existente",
                        "🆕 Criar Nova Conta"
                    }));
            
            // dependendo da escolha do user, chama o metodo para cada coisa
            if (escolha.Contains("Existente"))
            {
                UsarContaExistente();
            }
            else
            {
                CriarNovaConta();
            }
        }
        
        // Aqui já e um metodo para criar uma conta completamente nova
        static void CriarNovaConta()
        {
            Console.Clear();
            Titulo();
            
            var panelTitulo = new Panel(Align.Center(new Markup(
                "[bold green]CRIAR NOVA CONTA[/]\n\n" +
                "Bem-vindo! Vamos configurar sua nova conta bancária.")))
                .RoundedBorder()
                .BorderColor(Color.Green);
            AnsiConsole.Write(Align.Center(panelTitulo));
            Console.WriteLine();
            
            // Pede o nome do user
            Console.Write("Digite seu nome: ");
            nome = Console.ReadLine();
            
            // Pede a idade do user
            Console.Write("Digite sua idade: ");
            idade = int.Parse(Console.ReadLine());
            
            //Um capricho... aqui e um aviso para menores de idade
            if (idade < 18)
            {
                Console.WriteLine();
                var panelAviso = new Panel(Align.Center(new Markup(
                    "[red3]⚠️  AVISO![/]\n\n" +
                    "Menores de idade precisam de autorização\n" +
                    "e supervisão de um adulto responsável.")))
                    .RoundedBorder()
                    .BorderColor(Color.Red3);
                AnsiConsole.Write(Align.Center(panelAviso));
                Console.WriteLine();
                Console.WriteLine("Pressione qualquer tecla para continuar...");
                Console.ReadKey();
            }
            
            // Cria pasta única para esta pessoa (baseado no nome)
            // ai remove espaços e deixa tudo minúsculo para criar o nome da pasta
            // Por exemplo "PSI 10H" vira "psi_10h"
            string nomePasta = nome.Replace(" ", "_").ToLower(); // ToLower() = deixa tudo em minúsculas
                                    // Replace(" ", "_") = substitui espaços por underscore
                                    
           /***************************************
           Criando as pastas para a conta do user
           ***************************************/
           
            // Path.Combine junta dois caminhos
            // Tipo, "contas" + "PSI_10H" = "contas/PSI_10H"
            string caminhoCompleto = Path.Combine(pastaContas, nomePasta);
            
            // Verifica se já existe uma conta com este nome
            if (Directory.Exists(caminhoCompleto)) // Se a pasta já existe
            {
                Console.Clear();
                var panelErro = new Panel(
                    $"[red]❌ JÁ EXISTE UMA CONTA COM O NOME \"{nome}\"![/]\n\n" +
                    "Por favor, use um nome diferente ou acesse a conta existente.")
                    .Border(BoxBorder.Rounded)
                    .BorderColor(Color.Red);
                AnsiConsole.Write(Align.Center(panelErro)); //apresenta o painel com a mensagem de erro
                Console.WriteLine();
                Console.ReadKey();
                SistemadeContas(); // então volta ao menu de contas
                return; //depois sai da condição
            }
            
            // Cria a pasta da conta no computador se ela já não existir
            Directory.CreateDirectory(caminhoCompleto);
            
            /* e agora definir onde ficarão os 3 arquivos desta conta:
            1. saldo.txt = guarda o saldo
            2. transacoes.txt = guarda o histórico
            3. dados.txt = guarda nome e idade 
            */
            arquivoSaldo = Path.Combine(caminhoCompleto, "saldo.txt");
            arquivoTransacoes = Path.Combine(caminhoCompleto, "transacoes.txt");
            arquivoDados = Path.Combine(caminhoCompleto, "dados.txt");
            
            /* Salva os dados básicos da pessoa no arquivo
            Formato: "Beatriz|15" (nome e idade separados por | para o computador reconhecer a divisão 
            (alternativa em vez de underscore))
            */
            File.WriteAllText(arquivoDados, $"{nome}|{idade}");
            // "WriteAllText" vai escrever o texto no arquivo (ou então cria o arquivo se não existir)
            
            // Inicia a conta com saldo 0
            userbalance = 0;
            SalvarSaldo();
            
            // Mensagem de sucesso bonitinha ao concluir a criação da conta
            Console.Clear();
            var panelSucesso = new Panel(
                $"[bold green]✅ CONTA CRIADA COM SUCESSO![/]\n\n" +
                $"Titular: [cyan]{nome}[/]\n" +
                $"Idade: [cyan]{idade}[/]\n" +
                $"Saldo inicial: [green]0.00{euro}[/]")
                .Border(BoxBorder.Double)
                .BorderColor(Color.Green)
                .Header("🎉 Parabéns!");
            AnsiConsole.Write(Align.Center(panelSucesso));
            Console.WriteLine();
            Console.WriteLine("Pressione qualquer tecla para continuar...");
            Console.ReadKey();
        }
        
        // Aqui já ativa uma conta que já existe
        static void UsarContaExistente()
        {
            Console.Clear();
            Titulo();
            
            /* Aqui e aquela arrays do sistema de contas ainda, que analisa quantas pastas(contas) existem no computador
             e então e como pedir para ele apresentar as contas que tem
            */
            string[] contas = Directory.GetDirectories(pastaContas);
            
            //e aqui meio que 'apresenta' as contas existentes (se houver) para o user escolher
            string[] opcoesContas = new string[contas.Length + 1];                  // e a apresentação das contas como opções
            //e o Lenght novamente e para analisar a existencia das pastas          // +1 para a opção de voltar, não e obrigatorio, mas preferi assim pro
                                                                                    //user ter a opção de voltar em vez de ser obrigado a 
                                                                                    //escolher umas das contas mesmo que n queira
           
            
            // Aqui preenche as opções
            int i = 0;
            while (i < contas.Length) //como explico, aqui e um loop que vai analisar tudo de cada pasta uma por uma, e so assim então
            //vai apresentar a conta no menu, e basicamente isso, e so termina quando não houver mais pastas para analisar
            {
                string pasta = contas[i];
                string arquivoDadosConta = Path.Combine(pasta, "dados.txt"); //aqui procura o ficheiro onde foi armazenada as informações da conta
                
                if (File.Exists(arquivoDadosConta))//e então se o ficheiro existir ele começa a analisa o ficheiro
                {
                    // Lê os dados: nome|idade
                    string conteudo = File.ReadAllText(arquivoDadosConta); //aqui lê toda a informação junta, aquela lá "Beatriz|15"
                    //aqui ele ja divide o conteúdo, em vez de ser "Beatriz|15", vira [0]Beatriz, [1]15
                    string nomeConta = conteudo.Split('|')[0]; 
                    string idadeConta = conteudo.Split('|')[1];
                    
                    // Adiciona na lista de opções e apresenta o nome e a idade no menu 
                    opcoesContas[i] = $"👤 {nomeConta} ({idadeConta} anos)"; 
                }
                
                i++; //aqui passa para a proxima pasta, e então vai analisando até não ter mais
            }
            
            // Adiciona opção de voltar no menu
            opcoesContas[contas.Length] = "[red]⬅️  Voltar[/]";
            
            // Menu interativo com setinhas para o acesso das contas
            var escolha = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[green]Selecione a conta que deseja acessar:[/]")
                    .PageSize(10)
                    .AddChoices(opcoesContas));
            
            // Se escolheu voltar
            if (escolha.Contains("Voltar"))
            {
                SistemadeContas(); //volta para a parte de escolha entre criar ou entrar numa conta existente
                return;//então sai da condição
            }
            
            // ele meio que faz uma descodificação da frase, tirando emojis e números, deixando so o nome
            // Exemplo: " Beatriz (15 anos)" → "Beatriz"
            string nomeEscolhido = escolha.Split('(')[0].Replace("👤", "").Trim();
            
            // E então vai atras da pasta que corresponde ao nome
            //se o nome for "Professora Elsa", por exemplo, ele vai separar a frase como "professora_elsa", deixando
            //no padrão igual escrito na criação da conta par aajudar na busca da pasta da conta
            string nomePasta = nomeEscolhido.Replace(" ", "_").ToLower();
            string caminhoCompleto = Path.Combine(pastaContas, nomePasta); //este comando faz o programa indentificar
                                                                           //onde fica a conta usando o nome que ele "descodificou" anteriormente
                            //e ele vai atras da pasta das contas "contas/elsa"
                            
            // Define os caminhos dos arquivos, em suma, ele busca os dados salvos no "contas/elsa" e coloca eles nesses caminhos
            arquivoSaldo = Path.Combine(caminhoCompleto, "saldo.txt"); //o saldo que foi salvo anteriormente na pasta dessa conta, e direcionado para cá
            // so ai então ele e apresentado ao user no fim
            
            arquivoTransacoes = Path.Combine(caminhoCompleto, "transacoes.txt"); //mesma logica para os dois agora
            arquivoDados = Path.Combine(caminhoCompleto, "dados.txt"); //aqui também mesma lógica
            //(Obs!!!: o programa' ainda não leu mesmo os caminnhos, ele so foi direcionado para esses arquivos para so então realizar a leitura
            //e como dizer ao programa "O saldo, transações e os dados de Elsa, estão nesta gaveta!")
            
            // Carrega os dados: nome|idade
            string dadosLidos = File.ReadAllText(arquivoDados); //Agora sim, aqui que o programa vai pegar todos os dados da conta
            //selecionada, e vai atribuir para apresentar, e como pedir "Me devolva todo o texto que está ai dentro"
            
            //aqui repete novamente aquela divisão, se o arquivo contém um "Beatriz|15"
            //ele vai dividir as coisas novamente para "[0] Beatriz, [1] 15, atribuindo o [0] da array para nome,
            //e o [1] para a idade em forma de número e não texto"
            //(de acordo com a IA, posso chamar esse processo de "desserializar dados")
            nome = dadosLidos.Split('|')[0]; 
            idade = int.Parse(dadosLidos.Split('|')[1]);
            
            // Carrega o saldo finalmente
            CarregarSaldo();
            
            // Mensagem de boas-vindas após a leitura de tudo, e encaminhamento das pastas e arquivos...
            Console.Clear();
            var panelBemVindo = new Panel(
                $"[bold green]BEM-VINDO DE VOLTA, {nome.ToUpper()}![/]\n\n" +
                $"👤 Idade: [cyan]{idade}[/] anos\n" +
                $"💰 Saldo atual: [green]{userbalance:F2}{euro}[/]")
                .Border(BoxBorder.Double)
                .BorderColor(Color.Green)
                .Header("✨ Login Realizado");
            AnsiConsole.Write(Align.Center(panelBemVindo));
            Console.WriteLine();
            Console.WriteLine("Pressione qualquer tecla para continuar...");
            Console.ReadKey();
        }

        /********************************************************************
                 MÉTODOS DE SALVAMENTO E CARREGAMENTO DE DADOS
        *********************************************************************/
        
        // Carrega o saldo salvo no arquivo (se existir)
        // Se não existir, começa com saldo 0
        static void CarregarSaldo()
        {
            //esta parte foi feita pra garantir que o programa não apague nada fazendo ele verificar os arquivos salvos do user
            if (File.Exists(arquivoSaldo)) // Verifica se o arquivo existe
            {
                string conteudo = File.ReadAllText(arquivoSaldo); // Lê todo o conteúdo guardado no arquivo
                userbalance = float.Parse(conteudo); //le o saldo guardado e converte o texto para número
            }
            else
            {
                userbalance = 0; // Se não existe nenhum saldo anteriormente, ele inicia em 0
            }
        }

        // Salva o saldo atual no arquivo
        // Isso garante que o saldo fica guardado mesmo depois de fechar o programa
        static void SalvarSaldo()
        {
            File.WriteAllText(arquivoSaldo, userbalance.ToString());
            //aqui ele Escreve, ou reescreve o saldo do user... o "ToString" converte o valor númerico atualizado, para texto
            //isso e para a recolha de dados no arquivo depois não der erro, em vez de o programa ler assim ex: 15 como
            //um número inteiro ele vai ler "15" em forma de texto, e depois faz a conversão
        }

        // Salva uma transação no histórico
        // Formato: Data|Hora|Tipo|Descrição|Valor|SaldoDepois
        // Exemplo: 27/01/2026|14:30:45|Depósito|Depósito em dinheiro|500.00|500.00
        static void SalvarTransacao(string tipo, float valor, string descricao)
        { //este metódo não apresenta nada, ele so guarda as informações 
            // Cria uma linha com todos os dados separados por "|"
            string linha = $"{DateTime.Now:dd/MM/yyyy}|{DateTime.Now:HH:mm:ss}|{tipo}|{descricao}|{valor:F2}|{userbalance:F2}";
            //"DateTime.Now" e um comando que envolve data e horas atuais do sistema, onde tem "dd/MM/yyyy" envolve a data do dia mesmo
            //onde tem "HH:mm:ss" envolve as horas, min, e segundos 
            
            // Adiciona essa linha no final do arquivo (sem apagar o que já existe), so mesmo para salvar, e mesmo que o user saia
            //o histórico permanecera
            File.AppendAllText(arquivoTransacoes, linha + Environment.NewLine);
        }

        /********************************************************************
                MÉTODOS DE INTERFACE - Design bonito com Spectre.Console
        ********************************************************************/
        
        // Mostra o título "Banco" em letras grandes e verdes
        static void Titulo()
        {
            AnsiConsole.Write(
                new FigletText("Banco") // Texto em ASCII art grande
                    .Centered() // Centralizado
                    .Color(Color.Green)); // Cor verde
        }

        /********************************************************************
                                 MENU PRINCIPAL
        ********************************************************************/

        // Mostra o menu bonito com todas as opções
        // Retorna um número de 1 a 7 dependendo do que o usuário escolher
        static int MenuPrincipal()
        {
            Console.Clear(); // Limpa a tela
            Titulo(); // Mostra o logo

            // Formata o nome com somente a primeira letra maiúscula independente de como o user inserir
            // (ex: "eLsA" vira "Elsa")
            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo; //este comando representa as regras culturais do sistema
            //"O CurrentCulture" e a cultura(idioma) onde o programa está a correr
            //o "TextInfo" e a ferramenta dessa cultura que sabe como converter letras para Maiúsculas/Minúsculas,
            //e como capitalizar palavras corretamente
            
            string nomeFormatado = textInfo.ToTitleCase(nome.ToLower());
            //o "ToTitleCase" não corrige letras erradas, ele só coloca maiúscula na primeira letra de cada palavra, por isso usamos o
            //"ToLower" ele põe tudo em minúsculas e so depois o "ToTitleCase", põe a letra incial em maiúsculas
            //(o unico erro e que não fuciona para todos os tipos de idioma, como nomes estrangeiros como um "McDonald" ou "da Silva"
            //no caso do "da Silva", o "da" também teria a letra inicial em maiúscula, então nesses casos tem de se usar outra ferramenta
            //(que ainda não sei qual é))

            // Painel que mostra o saldo do usuário
            var painelSaldo = new Panel(
                    Align.Center(
                        new Markup(
                            $"[bold]Bem vindo[/] [Cyan]{nomeFormatado}[/]...Seu Saldo é: [green]{userbalance:F2}{euro}[/]")))
                .Header("[yellow]💳 Sua Conta[/]") // Cabeçalho amarelo
                .Border(BoxBorder.Double) // Borda dupla
                .BorderColor(Color.Yellow); // Cor amarela

            AnsiConsole.Write(painelSaldo);//apresenta o painel

            // Menu interativo com setinhas - você navega com setas do teclado
            var opcao = AnsiConsole.Prompt( //opcao não e um número certo, isso e enviado para a cadeia de if's abaixo
                new SelectionPrompt<string>()
                    .Title("[green]Escolha uma opção:[/]")
                    .PageSize(10)
                    .MoreChoicesText("[grey](Mova para cima e para baixo para ver mais opções)[/]")
                    .AddChoices(new[]
                    {
                        "💰 Consultar Saldo",
                        "➕ Fazer Depósito",
                        "➖ Fazer Levantamento",
                        "💳 Pagar Contas",
                        "📅 Calendário de Gastos",
                        "🔄 Trocar de Conta",
                        "🚪 Sair"
                    }));

            // Converte a escolha (texto) em número
            // Usa o switch do main para decidir qual número retornar
            
            //ele converte a opção em texto, como o "Consultar Saldo", em um número e então envia para o switch do main
            int numeroOpcao = 0;
            
            //optei pelo o uso de if's, por que so uma opção pode ser verdadeira, assim que uma delas for "true" ela não verifica
            //as outras 

            if (opcao.Contains("Consultar Saldo")) //então dentro do prompt da var opcao, conter(".Contais") o texto por exemplo
            //"Consultar Saldo", vai ser atribuido o valor de 1, para a opção, e então eviada para o switch case do main
            //em outras palavras posso explicar como "Este texto(opcao) contem a frase "Consultar Saldo", se sim retorna "true"
                numeroOpcao = 1;
            else if (opcao.Contains("Fazer Depósito"))
                numeroOpcao = 2;
            else if (opcao.Contains("Fazer Levantamento"))
                numeroOpcao = 3;
            else if (opcao.Contains("Pagar Contas"))
                numeroOpcao = 4;
            else if (opcao.Contains("Calendário de Gastos"))
                numeroOpcao = 5;
            else if (opcao.Contains("Trocar de Conta"))
                numeroOpcao = 6;
            else if (opcao.Contains("Sair"))
                numeroOpcao = 7;

            return numeroOpcao; //devolve o valor para o método, que posteriormente será atribuido no main...
        }

        /********************************************************************
                     LEVANTAMENTO - Tirar dinheiro da conta
        ********************************************************************/

        // Permite o usuário retirar dinheiro da conta
        // Verifica se tem saldo suficiente antes de permitir
        static void Levantamento()
        {
            Console.Clear();

            // Título da secção
            var titulo = new Rule("[yellow]💸 LEVANTAMENTO[/]")
                .RuleStyle("green");
            AnsiConsole.Write(titulo);

            // Mostra o saldo atual
            var panel = new Panel($"[bold]Saldo Atual:[/] [green]{userbalance:F2}{euro}[/]")
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Green);
            AnsiConsole.Write(Align.Center(panel));

            // Pergunta quanto quer levantar
            var panel2 = new Panel($"Insira a Quantia que deseja retirar: ")
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Yellow);
            AnsiConsole.Write(Align.Center(panel2));
            Console.WriteLine();

            levantamento = float.Parse(Console.ReadLine()); // Lê o valor que o utilizador inserir

            // Verificação se o utilizador tem a quantia o suficiente, ou se ultrapassa do seu saldo atual
            if (levantamento > userbalance)
            {
                Console.Clear();

                // Se o saldo for insuficiente mostra o erro de "saldo insuficiente"
                var erroPanel = new Panel(
                    $"[red]❌ SALDO INSUFICIENTE![/]\n\n" +
                    $"Valor solicitado: [yellow]{levantamento:F2}{euro}[/]\n" +
                    $"Seu saldo: [red]{userbalance:F2}{euro}[/]\n" +
                    $"Faltam: [red]{(levantamento - userbalance):F2}{euro}[/]") //capricho de mostrar quanto dinheiro falta para
                    //o utilizador conseguir realizar o levantamento
                    .Border(BoxBorder.Rounded)
                    .BorderColor(Color.Red);

                AnsiConsole.Write(Align.Center(erroPanel)); //apresenta o painel com a mensagem de erro
                Console.ReadKey();
                return; // Sai do método sem fazer nada caso o user tenha saldo suficiente
            }
            
            //se o utilizador tiver dinheiro suficiente o metódo continua 
            
            userbalance -= levantamento; // Subtrai do saldo o valor inserido
            SalvarSaldo(); // Salva o novo saldo no arquivo no esqueminha 
            SalvarTransacao("Levantamento", levantamento, "Levantamento em dinheiro"); //adiciona uma nova linha no arquivo
            //para registar a alteração do saldo do user depois no "calendario de gastos"

            Console.Clear();//limpeza na consola

            // Mostra confirmação de sucesso após o processo de levantamento
            var panelValorAtualizado = new Panel(
                    $"[bold green]✅ LEVANTAMENTO REALIZADO![/]\n\n" +
                    $"💰 Valor retirado: [red]-{levantamento:F2}{euro}[/]\n" +
                    $"━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                    $"💳 [green]Novo Saldo: {userbalance:F2}{euro}[/]")
                .Border(BoxBorder.Double)
                .BorderColor(Color.Green);
            AnsiConsole.Write(Align.Center(panelValorAtualizado));
            Console.WriteLine();
            Console.ReadKey();
        }

        /********************************************************************
                    DEPÓSITO - Adicionar dinheiro à conta
        ********************************************************************/

        // Permite o usuário adicionar dinheiro à conta
        // Não precisa verificar nada, só adiciona!
        static void Deposito()
        {
            Console.Clear();

            // Título da secção
            var titulo = new Rule("[yellow]💵 DEPÓSITO[/]")
                .RuleStyle("green");
            AnsiConsole.Write(titulo);

            // Pergunta quanto o user quer depositar na conta
            var panel = new Panel($"Insira a Quantia que deseja adicionar: ")
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Green);
            AnsiConsole.Write(Align.Center(panel));//apresenta o panel ao meio da consola
            Console.WriteLine();

            deposito = float.Parse(Console.ReadLine()); // Lê o valor inserido
            float saldoAnterior = userbalance; // Guarda o saldo antes (para mostrar depois)

            userbalance += deposito; // so então adiciona ao saldo
            SalvarSaldo(); // Salva no arquivo o novo saldo
            SalvarTransacao("Depósito", deposito, "Depósito em dinheiro"); //Mesma coisa com o levantamento
            //so que agora e no arquivo do deposito

            Console.Clear();

            // Mostra confirmação de sucesso ao inserir o valor
            var panelValorAtualizado = new Panel(
                    $"[bold green]✅ DEPÓSITO REALIZADO COM SUCESSO![/]\n\n" +
                    $"💰 Valor adicionado: [green]+{deposito:F2}{euro}[/]\n" +
                    $"━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                    $"💵 Saldo anterior: [yellow]{saldoAnterior:F2}{euro}[/]\n" +
                    $"💳 [green]Novo Saldo: {userbalance:F2}{euro}[/]")
                .Border(BoxBorder.Double)
                .BorderColor(Color.Green);
            AnsiConsole.Write(Align.Center(panelValorAtualizado));
            Console.WriteLine();
            Console.ReadKey();
        }

        /********************************************************************
                    CONSULTAR SALDO - Ver quanto dinheiro tem
        ********************************************************************/

        // Mostra o saldo em 'detalhes' e também dá opções rápidas de fazer depósito ou levantamento, ou então
        // so voltar para o menu
        static void ConsultarSaldo()
        {
            Console.Clear();

            // Título da secção
            var titulo = new Rule("[yellow]💰 CONSULTA DE SALDO[/]")
                .RuleStyle("green");
            AnsiConsole.Write(titulo);

            // Mostra o saldo em destaque
            var panel = new Panel($"O seu Saldo Atual é: [green]{userbalance:F2}{euro}[/]")
                .Border(BoxBorder.Double)
                .BorderColor(Color.Yellow)
                .Header("💳 Saldo Disponível");
            AnsiConsole.Write(Align.Center(panel));
            Console.WriteLine();

            // Menu interativo com setinhas para os atalhos rápidos
            var escolha = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]O que deseja fazer?[/]")
                    .AddChoices(new[]
                    {
                        "⬅️  Voltar ao Menu Principal",
                        "➕ Fazer um Depósito",
                        "➖ Fazer um Levantamento"
                    }));

            // Decide o que fazer baseado na escolha
            if (escolha.Contains("Depósito"))
            {
                Deposito();
            }
            else if (escolha.Contains("Levantamento"))
            {
                Levantamento();
            }
            // Se escolheu voltar, não faz nada so volta ao menu automaticamente
        }

        /********************************************************************
                PAGAR CONTAS - Pagar contas com valores aleatórios
        ********************************************************************/

        // Metodo para o Sistema de pagamento de contas
        // Os valores são gerados aleatoriamente toda vez (pesquisei valores aproximados,
        // e na geração aleatoria dos números impus os limites mais realistas que pensei)
        static void PagarContas()
        {
            Console.Clear();

            // Gerador de números aleatórios
            Random random = new Random();

            // Título da seção
            var titulo = new Rule("[yellow]💳 PAGAMENTO DE CONTAS[/]")
                .RuleStyle("green");
            AnsiConsole.Write(titulo);

            // Mostra o saldo disponível
            var painelSaldo = new Panel($"[bold]Saldo Disponível:[/] [green]{userbalance:F2}{euro}[/]")
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Green);
            AnsiConsole.Write(Align.Center(painelSaldo));
            Console.WriteLine();

            // Cria 7 contas para pagar com valores aleatórios citados anteriormente
            string conta1Nome = "💡 Conta de Luz - EDP";//nome da conta
            float conta1Valor = random.Next(35, 120) + (float)Math.Round(random.NextDouble(), 2);
            /*
            -vamos la, vou explicar de forma lógica, não por ordem que aparece, o "random.Next" vai gerar numeros
            aleatorios entre os parametros que eu selecionar, nesse caso vão ser numeros entre 35 - 119, essa ferramenta
             n conta o último número, e sempre um a menos do que indicado nos parâmetros (pelo menos que eu saiba)
            -O"random.NextDouble" gera valores decimais, ate o número ser inteiro, ex: 0 - 0.999...-(ate se tornar o numero inteiro como o 1 e assim vai)
            mas como essa função devolve um double, devemos inicialmente por o float, já que a variavel e um float
            o comando ira automaticamente fazer a conversão
            -O "Math.Round(),2" ira arrendondar "2" duas casas decimais, pois foi o falor que pedimos para ele arreondar
            e assim se repete em todas as seguintes opções
            */
            string conta2Nome = "💧 Conta de Água - EPAL";
            float conta2Valor = random.Next(20, 65) + (float)Math.Round(random.NextDouble(), 2);

            string conta3Nome = "📱 Telemóvel - MEO";
            float conta3Valor = random.Next(15, 50) + (float)Math.Round(random.NextDouble(), 2);

            string conta4Nome = "🌐 Internet - NOS";
            float conta4Valor = random.Next(30, 75) + (float)Math.Round(random.NextDouble(), 2);

            string conta5Nome = "📺 TV por Cabo - Vodafone";
            float conta5Valor = random.Next(25, 60) + (float)Math.Round(random.NextDouble(), 2);

            string conta6Nome = "🏠 Condomínio";
            float conta6Valor = random.Next(100, 350) + (float)Math.Round(random.NextDouble(), 2);

            string conta7Nome = "🔥 Gás Natural";
            float conta7Valor = random.Next(25, 90) + (float)Math.Round(random.NextDouble(), 2);

            //Aqui crio uma array de opções para o menu interativo com setinhas
            string[] opcoesContas = new string[8]; // 7 contas + 1 opção de voltar
            /*
            então vai se chamando as variaveis antes declaradas e o valor aleatorio gerado
            para dentro da array
            */
            opcoesContas[0] = $"{conta1Nome} - [green]{conta1Valor:F2}{euro}[/]"; 
            opcoesContas[1] = $"{conta2Nome} - [green]{conta2Valor:F2}{euro}[/]";
            opcoesContas[2] = $"{conta3Nome} - [green]{conta3Valor:F2}{euro}[/]";
            opcoesContas[3] = $"{conta4Nome} - [green]{conta4Valor:F2}{euro}[/]";
            opcoesContas[4] = $"{conta5Nome} - [green]{conta5Valor:F2}{euro}[/]";
            opcoesContas[5] = $"{conta6Nome} - [green]{conta6Valor:F2}{euro}[/]";
            opcoesContas[6] = $"{conta7Nome} - [green]{conta7Valor:F2}{euro}[/]";
            opcoesContas[7] = "[red]❌ Voltar ao Menu[/]";

            // Menu interativo com setinhas
            var escolha = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]Selecione a conta que deseja pagar:[/]")
                    .PageSize(10)
                    .AddChoices(opcoesContas));

            //Se o user escolheu voltar
            if (escolha.Contains("Voltar"))
            {
                return;//da um return para o menu principal
            }

            // Variáveis para guardar qual conta foi escolhida
            string contaNome = "";//variavel string com um valor de 0, porr isso nada entre as aspas
            float contaValor = 0;

            /* aqui tem mais ou menos a mesma logica da cadeia de if's no menu principal, so que nesse caso não iremos chamar
             um retorno posteriormente, ate por que e um metodo de procedimento e não de função
             */
            if (escolha.Contains(conta1Nome)) //Mesma logica, "Se o Texto "escolha" conter a frase "(parametro)",
                                              //execute o comando..."
            {
                contaNome = conta1Nome;//aqui vamos atribuindo as contas caso o ".Contains" for true
                contaValor = conta1Valor; //e aqui os valores aleatorios gerados
            }
            else if (escolha.Contains(conta2Nome))
            {
                contaNome = conta2Nome;
                contaValor = conta2Valor;
            }
            else if (escolha.Contains(conta3Nome))
            {
                contaNome = conta3Nome;
                contaValor = conta3Valor;
            }
            else if (escolha.Contains(conta4Nome))
            {
                contaNome = conta4Nome;
                contaValor = conta4Valor;
            }
            else if (escolha.Contains(conta5Nome))
            {
                contaNome = conta5Nome;
                contaValor = conta5Valor;
            }
            else if (escolha.Contains(conta6Nome))
            {
                contaNome = conta6Nome;
                contaValor = conta6Valor;
            }
            else if (escolha.Contains(conta7Nome))
            {
                contaNome = conta7Nome;
                contaValor = conta7Valor;
            }

            //Mass um detalhe, so será possivel pagar as contas se o user tiver o saldo necessário
            if (userbalance < contaValor)
            {
                Console.Clear();

                // Mostra erro detalhado e com um aspecto bonitinho
                var erroPanel = new Panel(
                        $"[red]❌ SALDO INSUFICIENTE![/]\n\n" +
                        $"Valor da conta: [yellow]{contaValor:F2}{euro}[/]\n" +
                        $"Seu saldo: [red]{userbalance:F2}{euro}[/]\n" +
                        $"Faltam: [red]{(contaValor - userbalance):F2}{euro}[/]")
                    .Border(BoxBorder.Rounded)
                    .BorderColor(Color.Red);

                AnsiConsole.Write(Align.Center(erroPanel)); //alinha o painel de erro ao centro da consola
                Console.ReadKey();
                return; // Volta ao menu sem fazer nada
            }

            //Pede confirmação do pagamento usando menu interativo
            var confirma = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[yellow]Confirma o pagamento de[/] [green]{contaValor:F2}{euro}[/]?")
                    .AddChoices(new[]
                    {
                        "✅ Sim, confirmar pagamento",
                        "❌ Não, cancelar"
                    }));

            if (confirma.Contains("Sim"))
            {
                //Aqui so guarda o saldo antes de descontar, so vai ser mostrado depois
                float saldoAnterior = userbalance;

                //Desconta do saldo
                userbalance -= contaValor;
                
                SalvarSaldo(); //Salva o novo saldo no arquivo
                SalvarTransacao("Pagamento", contaValor, contaNome); //Adiciona uma nova linha no arquivo,
                                                                     //e regista no historico

                Console.Clear();//limpezinha na consola

                //Animação de processamento (só para ficar bonitinho) mostrando "Processando pagamento..." com animação de pontos girando
                AnsiConsole.Status()
                    .Start("Processando pagamento...", ctx =>
                    {
                        ctx.Spinner(Spinner.Known.Dots); //Tipo de animação
                        ctx.SpinnerStyle(Style.Parse("green")); //Cor verde
                        Thread.Sleep(2000); //Espera de 2 segundos
                    });

                //Mostra comprovante de pagamento
                var sucesso = new Panel(
                        $"[bold green]✅ PAGAMENTO REALIZADO COM SUCESSO![/]\n\n" +
                        $"📄 Conta: {contaNome}\n" +
                        $"💰 Valor pago: [red]-{contaValor:F2}{euro}[/]\n" +
                        $"━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                        $"💵 Saldo anterior: [yellow]{saldoAnterior:F2}{euro}[/]\n" +
                        $"💳 [green]Novo saldo: {userbalance:F2}{euro}[/]")
                    .Border(BoxBorder.Double)
                    .BorderColor(Color.Green)
                    .Header("🧾 Comprovante de Pagamento");

                AnsiConsole.Write(Align.Center(sucesso));
                Console.WriteLine();

                var panelSaida = new Panel("Pressione qualquer tecla para voltar ao menu...")
                    .Border(BoxBorder.Rounded)
                    .BorderColor(Color.Blue);
                AnsiConsole.Write(Align.Center(panelSaida));

                Console.ReadKey();
            }
            else
            {
                //Se o user clicou em "Não, Cancelar", mostra mensagem de cancelamento
                AnsiConsole.MarkupLine("\n[yellow]⚠️  Pagamento cancelado[/]");
                Thread.Sleep(1500); // Espera 1.5 segundos antes de voltar para o menu
            }
        }

        /*********************************************************************
           CALENDÁRIO DE GASTOS - Histórico de todas as transações
        *********************************************************************/
        
        /*
         Mostra TODAS as transações feitas, depósitos, levantamentos, pagamentos
        com data, hora, tipo, descrição, valor e saldo após qualquer ação no saldo,
        também calcula estatísticas, total depositado, total gasto...e assim vai
        */
        static void CalendarioDeGastos()
        {
            Console.Clear();
            
            //Título da secção
            var titulo = new Rule("[yellow]📅 CALENDÁRIO DE GASTOS[/]")
                .RuleStyle("green");
            AnsiConsole.Write(titulo);

            //Verifica se existe o arquivo de histórico para apresentar
            if (!File.Exists(arquivoTransacoes))
            {
                //Se não existe, mostra mensagem que está vazio
                var panelVazio = new Panel(
                        "[yellow]Ainda não há transações registradas.[/]\n\n" +
                        "Faça depósitos, levantamentos ou pague contas para ver seu histórico aqui!")
                    .Border(BoxBorder.Rounded)
                    .BorderColor(Color.Yellow)
                    .Header("📊 Histórico Vazio");
                AnsiConsole.Write(Align.Center(panelVazio));
                Console.WriteLine();
                Console.ReadKey();
                return;//retorna para o menu
            }
            
            string[] linhas = File.ReadAllLines(arquivoTransacoes); //este array lê todas as linhas do arquivo, e cada linha
            //do arquivo das transações vira uma nova posição no array
            
            if (linhas.Length == 0)//mas se existe um arquivo, mas não possui nenhum resgitso(linha) nele
            {
                //o comando ira apresentar que não há nenhum historico, isso e para diminuir a margem de erro caso algum 
                //arquivo de erro, em vez de fechar o programa, ira apresentar essa mensagem e uma caxinha para melhor aspecto
                //grafico
                var panelVazio = new Panel(
                        "[yellow]Ainda não há transações registradas.[/]\n\n" +
                        "Faça depósitos, levantamentos ou pague contas para ver seu histórico aqui!")
                    .Border(BoxBorder.Rounded)
                    .BorderColor(Color.Yellow)
                    .Header("📊 Histórico Vazio");
                AnsiConsole.Write(Align.Center(panelVazio));
                Console.WriteLine();
                Console.ReadKey();
                return;//então ao presionar qualquer tecla, o utilizador voltará para o menu, ja que não há dados, vai
                       //cair pra fora do metodo e não ira calcular as estatísticas e apresentar o histórico
                       
            }//depois desse return o codigo desse metodo so prossegue se houver dados armazenados na array que le os arquivos
            
            // mas se houver dados o programa ira calcular as estatisticas
            
            //aqui e a declaração das variaveis locais para armazenar os valores enquanto o histórico e pecorrido
            float totalDepositado = 0; // Soma de todos os depósitos
            float totalGasto = 0; // Soma de todos os gastos
            int totalTransacoes = 0; // Número total de transações

            // Mostra cabeçalho da tabela
            Console.WriteLine("\n═══════════════════════════════════════════════════════════════════════");
            Console.WriteLine("  DATA       HORA     TIPO          DESCRIÇÃO              VALOR    SALDO");
            Console.WriteLine("═══════════════════════════════════════════════════════════════════════");
            
            // Percorre cada linha do arquivo, o "-1" e por que ele pecorre do ultimo arquivo ao primeiro, ou seja, recente
            //ao ultimo
            int i = linhas.Length - 1;
            while (i >= 0)
            {
                string linha = linhas[i];//quantidade de linhas no arquivo
                
                if (!string.IsNullOrWhiteSpace(linha))//aqui ele ignora os espaços em branco no arquivo, seria uma lixeira
                {
                    string[] partes = linha.Split('|');// esta array vai cortar com um "|" toda vez apos uma string e
                                                       // separar como uma parte do array
                                                       /*ex: partes[0] = "12/01/2026" (split '|')
                                                        partes[1] = "14:32" (split '|')
                                                        partes[2] = "Depósito" (split '|')
                                                        partes[3] = "Depósito inicial" (split '|')
                                                        partes[4] = "50.00" (split '|')
                                                        partes[5] = "150.00" (split '|')
                                                        organizando cada informação sendo possivel para o programa trabalhar... */
                    
                    //Verifica se tem 6 partes que devem ser apresentadas, pois pode existir uma linha incompleta ou corrompida
                    if (partes.Length == 6)
                    {
                        //Extrai cada parte
                        string data = partes[0];
                        string hora = partes[1];
                        string tipo = partes[2];
                        string descricao = partes[3];
                        float valor = float.Parse(partes[4]); //aqui ele converte os valores de string, em valores númericos float na array
                        float saldoApos = float.Parse(partes[5]); //mesma coisa aqui

                        //Aqui calcula estatísticas
                        if (tipo == "Depósito") //se for deposito, e que entrou dinheiro
                        {
                            totalDepositado += valor; //total depositado seria ex: 50 + "Meu saldo Anterior antes do deposito"
                        }
                        else //Agora qualquer outra linha seria retirada de dinheiro, então subtrai-se esse valor
                        {
                            totalGasto += valor;
                        }
                        //toda linha valida conta como uma transação
                        totalTransacoes++;

                        // Define símbolo baseado no tipo
                        string sinal = tipo == "Depósito" ? "+" : "-"; //se for deposito usa o "+", senão, usa "-"
                        //e apenas para uma aparencia visual levemente mais dinâmica
                        
                        // Limita o tamanho da descrição da conta ate 17 caracteres, se a frase ultrapassar 20 char corta para "..."
                        //ex:"Pagamento da fatura men|sal de eletricidade da residência", e desformatar a tabela
                        //, a frase e limitada para melhor aspecto gráfico
                        //então fica "Pagamento da fatura men...[...]"
                        if (descricao.Length > 20)
                        {
                            descricao = descricao.Substring(0, 17) + "...";
                        }

                        //Mostra então a linha da transação formatada
                        Console.WriteLine($"{data} {hora}  {tipo,-12} {descricao,-20} {sinal}{valor,7:F2}€ {saldoApos,7:F2}€");
                        /*{tipo,-12} - alinha a frase à esquerda em 12 espaços
                         {descricao,-20} - alinha a frase à esquerda em 20
                         {valor,7:F2} - 2 casas decimais, alinhado a frase à direita
                         {saldoApos,7:F2} - mesmo formato do de cima
                        */
                    }
                }
                
                i--;
            }
            
            Console.WriteLine("═══════════════════════════════════════════════════════════════════════");

            //Aqui processa cada linha do histórico, separando os dados, exibindo as transações formatadas e acumulando
            //as estatísticas
            Console.WriteLine();
            var panelEstatisticas = new Panel(
                    $"[green]💰 Total Depositado:[/] [green]{totalDepositado:F2}{euro}[/]\n" +
                    $"[red]💸 Total Gasto:[/] [red]{totalGasto:F2}{euro}[/]\n" +
                    $"[yellow]📊 Número de Transações:[/] {totalTransacoes}")
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Green)
                .Header("📈 Estatísticas");

            AnsiConsole.Write(Align.Center(panelEstatisticas));
            Console.WriteLine();

            var panelSaida = new Panel("Pressione qualquer tecla para voltar ao menu...")
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Blue);
            AnsiConsole.Write(Align.Center(panelSaida));

            Console.ReadKey();
        }

        /*********************************************************************
                         MAIN - Finalmente juntar as coisas
        *********************************************************************/
        
        static void Main()
        {
            // Configura para mostrar caracteres especiais corretamente (emojis, €, etc)
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            
            //Chama o metodo do Sistemas de Contas, Criar, Ou utilizar conta existente
            SistemadeContas();
            
            int op; // Variável para guardar a opção das escolha do switch case
            
            //Loop infinito, o programa só para quando a opção "Sair" for pressionada
            while (true)
            {
                op = MenuPrincipal(); //lembrando da cadeia de if's la no metodo do menu principal, aqui que o valor do
                                      //return sera atribuido
                
                // Decide o que fazer baseado na escolha do menu princiapl usando switch
                switch (op)
                {
                    //nessa cadeia de case's,vão ser chamandos os metodos de acordo com as opções oferecidas ao user
                    case 1: // Consultar Saldo
                        ConsultarSaldo();
                        break;
                    case 2: // Fazer Depósito
                        Deposito();
                        break;
                    case 3: // Fazer Levantamento
                        Levantamento();
                        break;
                    case 4: // Pagar Contas
                        PagarContas();
                        break;
                    case 5: // Calendário de Gastos
                        CalendarioDeGastos();
                        break;
                    case 6: // Trocar de Conta
                        Console.Clear();
                        //carregamento bonitinho
                        var panelTrocar = new Panel(
                            "[yellow]Voltando ao menu de contas...[/]")
                            .Border(BoxBorder.Rounded)
                            .BorderColor(Color.Yellow);
                        AnsiConsole.Write(Align.Center(panelTrocar));
                        Thread.Sleep(1500);
                        SistemadeContas(); // Volta para escolher conta
                        break;
                    case 7: // Sair
                        Console.Clear();
                        
                        //Mensagem de despedida para o utilizador ao sair
                        TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
                        string nomeFormatado = textInfo.ToTitleCase(nome.ToLower());
                        
                        var despedida = new Panel(
                                $"[bold green]Obrigado por usar, {nomeFormatado}![/]\n\n" +
                                $"💰 Saldo Final: [green]{userbalance:F2}{euro}[/]\n\n" +
                                "Até breve! 👋")
                            .Border(BoxBorder.Double)
                            .BorderColor(Color.Green);
                        AnsiConsole.Write(Align.Center(despedida));
                        Console.WriteLine();
                        Console.WriteLine("Pressione qualquer tecla para sair...");
                        Console.ReadKey();
                        return; // Encerra o programa completamente
                    
                    default: //Caso o utilizador não escolha nenhuma das opções oferecidas (diminuir margem de erro)
                             //aparecerá esta mensagem, então voltará ao menu novamente
                        AnsiConsole.MarkupLine("[rapid blink][red]Opção Inválida[/][/]");
                        Console.ReadKey();
                        break;
                }
            }
        }
    }

}
