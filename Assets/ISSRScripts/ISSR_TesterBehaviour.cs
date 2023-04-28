// ----------------------------------------------------------------------------------------
//  File: ISSR_TesterBehaviour.cs
//   Coded by Enrique Rendón: enriqueblender@gmail.com  for ISSR Unity API
//  Summary:
//    Testing Control Script:
//     Must be on an Object with "Tester" tag
// ----------------------------------------------------------------------------------------
using UnityEngine; 
using System.Collections;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ISSR_TesterBehaviour : MonoBehaviour
   {
    public GameObject Cam;
    public Text Etiqueta;
    public GameObject UICanvas;
    public GameObject UIPanel;
    public Text Texto;
    
    //public CharacterInfo charinfo;
    [Tooltip("Escena a probar, poner false para probar array de escenas")]
    public bool test_only_one_scene; // false para multples
    [Tooltip("Si al acabar de probar una escena de la lista espera a que se pulse return")]
    public bool press_return_for_next_scene;
    [Tooltip("Número de veces que hay que probar cada escena")]
    public int number_of_tests;
    [Tooltip("En caso de querer probar una sola escena poner aquí su índice en Build Settings")]
    public int scene_to_test_index;  // Va cambiando, poner escena a probar si solo es una
    String scene_name;
    [Tooltip("Para probar más de una escena poner aquí lista de índices de escenas en Build Settings")]
    public int[] scenes_to_test;
    public int current_test;   // number of test in a scene
    public bool if_blocked;
    public bool there_are_more_tests;

    
    public int[] completed_tests;  
    public float[] times; 
    public float[] mark; // Para el caso cooperativo
    public int[] points_per_scene;  
    public int current_scene;


    public ISSR_GameScenario scenario_type;

    // General data
    public int number_of_agents;
    public string TeamAName;
    public string TeamBName;
    public int total_time;

    public int total_points;

    public int total_bstones;
    public int total_sstones;
 
    // Accumulated data for a scene
    public float acc_elapsed_time;
    public int full_execs;

    public float acc_points_coop;
    public float acc_sstones_coop;
    public float acc_bstones_coop;

    public float acc_pointsA;
    public float acc_sstonesA;
    public float acc_bstonesA;

    public float acc_pointsB;
    public float acc_sstonesB;
    public float acc_bstonesB;
 

    public float full_points;
    public float full_sstones;
    public float full_bstones;

    public float full_points_coop;
    public float full_sstones_coop;
    public float full_bstones_coop;

    public float full_pointsA;
    public float full_sstonesA;
    public float full_bstonesA;

    public float full_pointsB;
    public float full_sstonesB;
    public float full_bstonesB;
    
    
    public String final_report;


    // Use this for initialization
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        DontDestroyOnLoad(Cam);
        DontDestroyOnLoad(UICanvas);
    }
    void Start ()
    {
        current_test = 0;
       
        if_blocked = false;
        there_are_more_tests = true;
        SceneManager.sceneLoaded += Scene_just_loaded;
        Etiqueta.text = "";

        current_scene = 0;

        if ((test_only_one_scene == false) && (scenes_to_test.Length==0))
        {
            Debug.LogError("No hay escenas definidas para ejecutar");
            return;
        }

        if ((scenes_to_test.Length> 0) && (test_only_one_scene== false))
        {
            times = new float[scenes_to_test.Length];
            mark = new float[scenes_to_test.Length];
            completed_tests = new int[scenes_to_test.Length];
            points_per_scene = new int[scenes_to_test.Length];
            scene_to_test_index = scenes_to_test[current_scene];
        }
        else
        {
            times = new float[1];
            mark = new float[1];
            completed_tests = new int[1];
            points_per_scene = new int[1];
            //scene_to_test_index = test_only_one_scene;
        }
     

        scene_name = SceneUtility.GetScenePathByBuildIndex(scene_to_test_index);
        Debug.LogFormat(scene_name);
        if (test_only_one_scene)
        {
            Texto.text = String.Format("Single Scene to test: \n \n \"{0}\" \n \n Press Return...",
                scene_name);
        }
        else
        {
            Texto.text = String.Format("First Scene to test\n (1 out of {1}): \n \n \"{0}\" \n \n Press Return...",
                scene_name, scenes_to_test.Length);
        }
        
    }

    private void Scene_just_loaded(Scene loaded_scene, LoadSceneMode arg1)
    {
        if (loaded_scene.buildIndex!=0)
        {
            UIPanel.SetActive(false);
            Cam.SetActive(false);
        }
        
    }

    IEnumerator LauchTest()
    {



        if ((press_return_for_next_scene == false) && (current_test == 0))
        {
            yield return new WaitForSeconds(4f);
        }
        Texto.text = String.Format("Escena:\n {1}\n\n Test número: {0}/{2}", current_test + 1, scene_name, number_of_tests);
        yield return new WaitForSeconds(1f);
        Etiqueta.text = string.Format("T{0}", current_test + 1);
        SceneManager.LoadScene(scene_to_test_index);
        
    }

    IEnumerator PrintFinalReport()
    {
        Texto.text = final_report;
        yield return new WaitForSeconds(2f);
    }
	
	// Update is called once per frame
	void Update ()
    {
		if (if_blocked==false)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                //Texto.fontSize = 60;
                ResetStatistics();
                

                if (there_are_more_tests)
                {
                    StartCoroutine(LauchTest());
                }
                else
                {
                    StartCoroutine(PrintFinalReport());
                }

                if_blocked = true;

            }
        }
	}

    public void InitError()
    {
        SceneManager.LoadScene(0);
        UIPanel.SetActive(true);
        Cam.SetActive(true);
        Texto.text = "Error cargando escena";
    }

    // Llamado por ISSRManager al comienzo de la ejecución de una escena
    public void GameParams(int number_of_agents, string TeamAName, string TeamBName, int total_time, int total_points,  int total_bstones, int total_sstones)
    {
        this.number_of_agents = number_of_agents;
        this.TeamAName = TeamAName;
        this.TeamBName = TeamBName;
        this.total_time = total_time;
        this.total_points = total_points;
        this.total_bstones = total_bstones;
        this.total_sstones = total_sstones;

        if ((TeamAName.Length!=0)&&(TeamBName.Length!=0) )
        {
            scenario_type = ISSR_GameScenario.Competitive;
        }
        else
        {
            scenario_type = ISSR_GameScenario.Cooperative;
        }
    }


    // Llamado por ISSRManager al final de una ejecución de un escena
    public void ExecData(int elapsed_time, int pointsA, int pointsB, int sstonesA, int sstonesB, int bstonesA, int bstonesB)
    {
        
        acc_pointsA += pointsA;
        acc_pointsB += pointsB;
        acc_sstonesA += sstonesA;
        acc_sstonesB += sstonesB;
        acc_bstonesA += bstonesA;
        acc_bstonesB += bstonesB;

        if (pointsA + pointsB == total_points)
        {
            full_execs++;
            acc_elapsed_time += elapsed_time;
        }

        current_test++;

        if (current_test < number_of_tests)  // Lanzar otro test de la misma escena
        {
            UIPanel.SetActive(true);
            Cam.SetActive(true);
            StartCoroutine(LauchTest());
        }
        else   // Se han acabado los tests de una escena
        {
            SceneManager.LoadScene(0);
            UIPanel.SetActive(true);
            Cam.SetActive(true);
            Texto.text = "Test Completado";
            if_blocked = false;
            PrintStatistics();
            ResetStatistics();
            Etiqueta.text = "ok";
            if_blocked = false;


            if ( (scenes_to_test.Length > 0)  && (test_only_one_scene ==false))
            {
                current_scene++;
                if (current_scene >= scenes_to_test.Length) // Terminadas todas las escenas
                {

                    //Texto.text = "TODO Completado";
                    if_blocked = false;
                    there_are_more_tests = false;
                    float tiempo_total = 0;
                    float totalizador = 0;
                    int completados = 0;
                    int dividir_completados = 0;

                    if (scenario_type == ISSR_GameScenario.Cooperative)
                    {
                        final_report = "------ EVALUACIÓN COOPERATIVA------\n";
                        for (int i = 0; i < scenes_to_test.Length; i++)
                        {
                            float avg_time_elapsed = times[i];
                            //scene_name = SceneUtility.GetScenePathByBuildIndex(i);
                            final_report = final_report + string.Format("Escena {2}# Pruebas completas:  {0} de {1}\n", completed_tests[i], number_of_tests, i);
                            completados = completados + completed_tests[i];

                            if (avg_time_elapsed > 1)
                            {
                                int minutes = Mathf.FloorToInt(avg_time_elapsed / 60);
                                int seconds = Mathf.RoundToInt(avg_time_elapsed - (minutes * 60));
                                dividir_completados++;
                                tiempo_total = tiempo_total + avg_time_elapsed;
                                final_report = final_report + string.Format(" Tiempo medio para completar: {0,2:D2}:{1,2:D2}\n", minutes, seconds);
                            }
                            else
                            {
                                final_report = final_report + string.Format(" Tiempo medio para completar: NO HAY\n");
                            }
                            totalizador = totalizador + (mark[i] * points_per_scene[i]) / full_points;
                            final_report = final_report + string.Format(" Nota: ## {0:0.0}-------------------\n", mark[i] / 10f);

                            //Debug.Log(final_report);                       
                        }


                        final_report = final_report + string.Format("\nTOTAL DE PRUEBAS COMPLETAS: {0} / {1}\n", completados, scenes_to_test.Length * number_of_tests);
                        final_report = final_report + string.Format("NOTA Ponderada GLOBAL: ## {0:0.0}\n", totalizador / 10);
                        if (dividir_completados == scenes_to_test.Length)
                        {
                            float avg_time_elapsed = tiempo_total;
                            int minutes = Mathf.FloorToInt(avg_time_elapsed / 60);
                            int seconds = Mathf.RoundToInt(avg_time_elapsed - (minutes * 60));
                            final_report = final_report + string.Format("TIEMPO MEDIO en completar todo: {0,2:D2}:{1,2:D2}\n", minutes, seconds);
                        }
                        else
                        {
                            final_report += string.Format("TIEMPO MEDIO en completar todo: NO HAY\n");
                            final_report += string.Format("Puntos: {0}/{1} -- ", full_points_coop, full_points * number_of_tests);
                            final_report += string.Format("PP: {0}/{1} --", full_sstones_coop, full_sstones * number_of_tests);
                            final_report += string.Format("PG: {0}/{1}\n", full_bstones_coop, full_bstones * number_of_tests);
                        }

                        Debug.Log(final_report);
                    }
                    else
                    {// Estadísticas globales competitivas.
                        final_report = "------ EVALUACIÓN COMPETITIVA------\n";

                        final_report += string.Format("Teams: {0} and {1}\n\n", TeamAName, TeamBName);
                        if (full_pointsA > full_pointsB)
                        {
                            final_report += string.Format("{0} WINS !!!\n\n", TeamAName);
                        }
                        else if (full_pointsA < full_pointsB)
                        {
                            final_report += string.Format("{0} WINS !!!\n\n", TeamBName);
                        }
                        else
                        {
                            final_report += string.Format("{0} and {1} DRAW (empatan)\n\n", TeamAName, TeamBName);
                        }

                        final_report += string.Format("{0}  -vs-  {1}\n", TeamAName, TeamBName);
                        final_report += string.Format("Total Points: {0}\n", full_points * number_of_tests);
                        final_report += string.Format("{0}  -vs-  {1}\n\n", full_pointsA, full_pointsB);
                        final_report += string.Format("Total SStones: {0}\n", full_sstones * number_of_tests);
                        final_report += string.Format("{0}  -vs-  {1}\n\n", full_sstonesA, full_sstonesB);
                        final_report += string.Format("Total BStones: {0}\n", full_bstones * number_of_tests);
                        final_report += string.Format("{0}  -vs-  {1}\n", full_bstonesA, full_bstonesB);
                    }


                }
                else
                {
                    scene_to_test_index = scenes_to_test[current_scene];
                    current_test = 0;
                    scene_name = SceneUtility.GetScenePathByBuildIndex(scene_to_test_index);
                    Debug.LogFormat(scene_name);

                    if (press_return_for_next_scene == false) 
                    {
                        if_blocked = true;
                        StartCoroutine(LauchTest());
                    }
                }
                    
            }
            else
            {
                there_are_more_tests = false;
                if_blocked = true;
                // Una sola escena, nada más que presentar
            }
        }
    }


    void ResetStatistics()
    {
        acc_elapsed_time = 0;
        acc_pointsA = 0;
        acc_pointsB = 0;
        acc_sstonesA = 0;
        acc_bstonesA = 0;
        acc_sstonesB = 0;
        acc_bstonesB = 0;
        full_execs = 0;
        current_test = 0;
    }
    // Se llama cuando acaban de ejecutar todos los tests de una escena
    void PrintStatistics()
    {
        String texto;

        // Texto.fontSize = 35;
        scene_name = SceneUtility.GetScenePathByBuildIndex(scene_to_test_index);
        texto = string.Format("Escena: {0}\n", scene_name);
        texto += string.Format("Finished matches: {0}/{1}\n", full_execs, number_of_tests);
        points_per_scene[current_scene] = total_points;
        full_points += total_points;
        full_bstones += total_bstones;
        full_sstones += total_sstones;

        if (full_execs > 0)
        {
            float avg_time_elapsed = acc_elapsed_time / full_execs;
            int minutes = Mathf.FloorToInt(avg_time_elapsed / 60);
            int seconds = Mathf.RoundToInt(  avg_time_elapsed - (minutes * 60));
            texto = texto + string.Format("Avg. Time to complete: {0,2:D2}:{1,2:D2}\n", minutes, seconds);
            times[current_scene] = avg_time_elapsed;
            
        }
        else
        {
            times[current_scene] = 0;
        }

        if (scenario_type == ISSR_GameScenario.Cooperative)
        {
            bool teamA = false;

            float avg_points;
            float avg_sstones;
            float avg_bstones;
            
            if (TeamAName.Length!=0)
            {
                teamA = true;
            }

            texto = texto + string.Format("Team: {0} with {1} agents\n\n", (teamA) ? TeamAName : TeamBName, number_of_agents);

            avg_points = (teamA) ? (acc_pointsA / number_of_tests) : (acc_pointsB / number_of_tests);

            texto = texto + string.Format("Avg Total Points: {0:0.0} / {1} = {2:0.0}%\n\n", avg_points, total_points,
              (teamA) ? (acc_pointsA * 100f/ (number_of_tests*total_points)) : (acc_pointsB * 100f / (number_of_tests * total_points)));
            mark[current_scene] = (teamA) ? (acc_pointsA * 100f / (number_of_tests * total_points)) : (acc_pointsB * 100f / (number_of_tests * total_points));
            completed_tests[current_scene] = full_execs;
            full_execs = 0;



            avg_sstones = (teamA) ? (acc_sstonesA / number_of_tests) : (acc_sstonesB / number_of_tests);
            if (total_sstones>0)
            {
                
                texto = texto + string.Format("Avg Small Stones: {0:0.0} / {1}\n", avg_sstones , total_sstones);
            }

            avg_bstones = (teamA) ? (acc_bstonesA / number_of_tests) : (acc_bstonesB / number_of_tests);
            if (total_bstones > 0)
            {
                
                texto = texto + string.Format("Avg  Big  Stones: {0:0.0} / {1}\n", avg_bstones, total_bstones);
            }


            full_points_coop +=( avg_points* number_of_tests);
            full_sstones_coop += (avg_sstones * number_of_tests);
            full_bstones_coop += (avg_bstones * number_of_tests);
        }
        else
        {
            completed_tests[current_scene] = full_execs;
            full_execs = 0;
            full_pointsA += acc_pointsA;
            full_pointsB += acc_pointsB;
            full_sstonesA += acc_sstonesA;
            full_sstonesB += acc_sstonesB;
            full_bstonesA += acc_bstonesA;
            full_bstonesB += acc_bstonesB;

            texto = texto + string.Format("Teams: {0} and {1} with {2} agents\n\n",  TeamAName , TeamBName, number_of_agents);
            if (acc_pointsA > acc_pointsB)
            {
                texto = texto + string.Format("Team {0} WINS!!!\n\n", TeamAName);
            }
            else
            {
                if (acc_pointsB > acc_pointsA)
                {
                    texto = texto + string.Format("Team {0} WINS!!!\n\n", TeamBName);
                }
                else
                {
                    texto = texto + "----DRAW---\n\n";
                }
            }
            texto = texto + string.Format("Results      {0}  -vs-  {1}\n", TeamAName, TeamBName);
            texto = texto + string.Format(" Avg Total Points: {0:0.0}  -  {1:0.0} out of {2}\n", acc_pointsA / number_of_tests, acc_pointsB / number_of_tests, total_points);
            if (total_sstones > 0)
            {
                texto = texto + string.Format("Avg Small Stones: {0:0.0}  -  {1:0.0} out of {2}\n", (acc_sstonesA / number_of_tests), (acc_sstonesB / number_of_tests), total_sstones);
            }
            if (total_bstones > 0)
            {
                texto = texto + string.Format("Avg  Big  Stones: {0:0.0}  -  {1:0.0} out of {2}\n", (acc_bstonesA / number_of_tests), (acc_bstonesB / number_of_tests), total_bstones);
            }

        }

        
        Debug.Log("------------------------------");

        if ((press_return_for_next_scene) || (current_scene == scenes_to_test.Length-1))
        {
            texto = texto + "\nPress Return...";
        }
        
        
        Debug.Log(texto);
        Texto.text = texto;


    }

   
}
