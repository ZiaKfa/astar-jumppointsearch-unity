using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Profiling;
using System;

public class Main : MonoBehaviour
{
    [Header("References")]
    public MapRenderer mapRenderer;
    public Dropdown dropdownMap;
    public GameObject loadingPanel;
    public Text loadingText;
    public GameObject benchmarkResultsPanel;
    public Text benchmarkResultsText;

    [Header("UI Input")]
    public InputField inputStartX;
    public InputField inputStartY;
    public InputField inputGoalX;
    public InputField inputGoalY;

    [Header("Buttons")]
    public Button btnRenderMap;
    public Button btnRunAStar;
    public Button btnRunJPS;
    public Button btnClearPath;
    public Button startBenchmark;
    public Button btnBenchmarkAll; // 🔵 NEW BUTTON

    // Internal State
    (int x, int y)[] currentPath;
    (int x, int y)[] lastOpenList;
    (int x, int y)[] lastClosedList;
    bool[,] currentMap;
    List<ScenReader.ScenItem> currentScenItems; 
    string mapsFolder;
    string[] availableMaps;

    int startX, startY, goalX, goalY;

    bool isMapRendered = false;
    bool isAstar = false;
    bool isJPS = false;

    // Struct Hasil
    public struct BenchmarkResult
    {
        public int index;
        public int startX, startY, goalX, goalY;

        public double timeAStar;
        public double timeJPS;
        public int openAStar;
        public int closedAStar;
        public long memoryAStar;
        public long memoryJPS;
        public int openJPS;
        public int closedJPS;
        public double optimalCost;
        public double pathLengthAStar;
        public double pathLengthJPS;
    }

    void Start()
    {
        loadingPanel.SetActive(false);
        benchmarkResultsPanel.SetActive(false);
        mapsFolder = Application.dataPath + "/Maps/";
        
        LoadMapListToDropdown();

        // Assign Listeners
        btnRenderMap.onClick.AddListener(RenderSelectedMap);
        btnRunAStar.onClick.AddListener(RenderAStar);
        btnRunJPS.onClick.AddListener(RenderJPS);
        btnClearPath.onClick.AddListener(ClearPath);
        
        // Single Benchmark
        startBenchmark.onClick.AddListener(() => StartCoroutine(RunSingleBenchmark()));

        // 🔵 All Benchmarks
        btnBenchmarkAll.onClick.AddListener(() => StartCoroutine(RunAllBenchmarks()));
    }

    void Update()
    {
        // State management UI interaction
        bool busy = loadingPanel.activeSelf;
        btnRenderMap.interactable = !busy;
        btnRunAStar.interactable = !busy && !isAstar && isMapRendered;
        btnRunJPS.interactable = !busy && !isJPS && isMapRendered;
        btnClearPath.interactable = !busy && (isAstar || isJPS);
        startBenchmark.interactable = !busy && isMapRendered;
        btnBenchmarkAll.interactable = !busy && availableMaps != null && availableMaps.Length > 0;
    }

    // ================= LOAD MAP FILES =================
    void LoadMapListToDropdown()
    {
        if (!Directory.Exists(mapsFolder)) Directory.CreateDirectory(mapsFolder);

        availableMaps = Directory.GetFiles(mapsFolder, "*.map")
                                 .Select(Path.GetFileNameWithoutExtension)
                                 .ToArray();

        dropdownMap.ClearOptions();
        dropdownMap.AddOptions(availableMaps.ToList());

        UnityEngine.Debug.Log($"Loaded {availableMaps.Length} maps.");
    }

    // ================= RENDER MAP LOGIC =================
    
    // Wrapper untuk button (mengambil dari dropdown)
    void RenderSelectedMap()
    {
        if (availableMaps.Length == 0) return;
        string mapName = availableMaps[dropdownMap.value];
        RenderMapByName(mapName);
    }

    // 🔵 Fungsi inti render agar bisa dipanggil loop
    void RenderMapByName(string mapName)
    {
        string path = mapsFolder + mapName + ".map";
        string scenPath = path.Replace("Maps", "Scens") + ".scen";

        if (!File.Exists(path))
        {
            UnityEngine.Debug.LogError("Map file not found: " + path);
            benchmarkResultsPanel.SetActive(true);
            benchmarkResultsText.text = "Error: Map file not found.";
            return;
        }

        currentMap = MapLoader.LoadMap(path);
        
        // Cek SCEN file
        if (File.Exists(scenPath))
        {
            currentScenItems = ScenReader.LoadScen(scenPath);
            UnityEngine.Debug.Log($"Loaded {currentScenItems.Count} scen items from {mapName}.scen");
            benchmarkResultsPanel.SetActive(true);
            benchmarkResultsText.text = $"Loaded {currentScenItems.Count} scen items from {mapName}.scen";
        }
        else
        {
            currentScenItems = new List<ScenReader.ScenItem>();
            UnityEngine.Debug.LogWarning($"SCEN file not found for {mapName}");
            benchmarkResultsPanel.SetActive(true);
            benchmarkResultsText.text = "Warning: SCEN file not found.";
        }

        mapRenderer.RenderFromArray(currentMap);
        isMapRendered = true;
        if(currentPath != null)
        {
            ClearPath();
        }
    }

    // =================== VISUALIZATIONS ===================
    void RenderAStar()
    {    
        if(currentPath != null) ClearPath();
        
        Stopwatch sw = new Stopwatch();
        sw.Start();
        if (!ParseInput()) return;
        
        var pathResult = AStar.FindPath(currentMap, startX, startY, goalX, goalY);
        lastOpenList = AStar.LastOpenList;
        lastClosedList = AStar.LastClosedList;

        currentPath = pathResult;
        isAstar = true; isJPS = false;

        mapRenderer.RenderOpenList(lastOpenList, currentMap);
        mapRenderer.RenderClosedList(lastClosedList, currentMap);
        mapRenderer.RenderPath(pathResult, currentMap);
        
        sw.Stop();
        UnityEngine.Debug.Log($"A* Finished. Cost: {AStar.LastFinalCost}. Time: {sw.Elapsed.TotalMilliseconds} ms");
        benchmarkResultsPanel.SetActive(true);
        benchmarkResultsText.text = $"A* Finished. Cost: {AStar.LastFinalCost}. Time: {sw.Elapsed.TotalMilliseconds} ms";
    }

    void RenderJPS()
    {
        if(currentPath != null) ClearPath();

        Stopwatch sw = new Stopwatch();
        sw.Start();
        if (!ParseInput()) return;

        var pathResult = JumpPointSearch.FindPath(currentMap, startX, startY, goalX, goalY);
        lastOpenList = JumpPointSearch.LastOpenList;
        lastClosedList = JumpPointSearch.LastClosedList;

        currentPath = pathResult;
        isJPS = true; isAstar = false;

        mapRenderer.RenderOpenList(lastOpenList, currentMap);
        mapRenderer.RenderClosedList(lastClosedList, currentMap);
        mapRenderer.RenderPath(pathResult, currentMap);

        sw.Stop();
        UnityEngine.Debug.Log($"JPS Finished. Cost: {JumpPointSearch.LastFinalCost}. Time: {sw.Elapsed.TotalMilliseconds} ms");
        benchmarkResultsPanel.SetActive(true);
        benchmarkResultsText.text = $"JPS Finished. Cost: {JumpPointSearch.LastFinalCost}. Time: {sw.Elapsed.TotalMilliseconds} ms";
    }

    void ClearPath()
    {
        mapRenderer.ClearPath(currentPath, currentMap);
        mapRenderer.ClearPath(lastOpenList, currentMap);
        mapRenderer.ClearPath(lastClosedList, currentMap);
        currentPath = null;
        isAstar = false;
        isJPS = false;
    }

    // =======================================================
    // 🔵 BENCHMARK LOGIC (REFACTORED)
    // =======================================================

    // 1. Single Map Benchmark (Dari Tombol UI)
    IEnumerator RunSingleBenchmark()
    {
        if (!isMapRendered)
        {
            loadingText.text = "Error: Map belum dirender.";
            yield break;
        }

        string mapName = availableMaps[dropdownMap.value];
        loadingPanel.SetActive(true);
        
        // Panggil core logic
        yield return StartCoroutine(ExecuteBenchmarkLogic(mapName));
        
        loadingPanel.SetActive(false);
    }

    // 2. All Maps Benchmark (Looping)
    IEnumerator RunAllBenchmarks()
    {
        loadingPanel.SetActive(true);
        loadingText.text = "Starting Bulk Benchmark...";
        yield return new WaitForSeconds(0.5f);

        int totalMaps = availableMaps.Length;

        for (int i = 0; i < totalMaps; i++)
        {
            string mapName = availableMaps[i];
            loadingText.text = $"Processing Map {i + 1}/{totalMaps}: {mapName}";
            
            // A. Load & Render Map (Wajib untuk setup currentMap)
            RenderMapByName(mapName);
            
            // Tunggu 1 frame agar rendering selesai (jika ada heavy process)
            yield return null; 

            // B. Jalankan Benchmark
            yield return StartCoroutine(ExecuteBenchmarkLogic(mapName));

            // C. Singgah sebentar sebelum ke map berikutnya
            yield return new WaitForSeconds(0.2f);
        }

        loadingText.text = "ALL BENCHMARKS COMPLETED!";
        yield return new WaitForSeconds(2f);
        loadingPanel.SetActive(false);
    }

    // 3. Core Logic Benchmark (Digunakan oleh Single maupun All)
    IEnumerator ExecuteBenchmarkLogic(string mapName)
    {
        if (currentScenItems == null || currentScenItems.Count == 0)
        {
            UnityEngine.Debug.LogWarning($"Skipping benchmark for {mapName}: No SCEN items.");
            yield break;
        }

        List<BenchmarkResult> results = new List<BenchmarkResult>();
        const int iterations = 10; 


        int totalCases = currentScenItems.Count;

        for (int i = 0; i < totalCases; i++)
        {
            loadingText.text = $"Benchmarking {mapName}\nCase {i + 1}/{totalCases}";
            yield return null; 

            var s = currentScenItems[i];
            BenchmarkResult r = new BenchmarkResult();

            r.index = i;
            r.startX = s.StartX; r.startY = s.StartY;
            r.goalX = s.GoalX; r.goalY = s.GoalY;
            r.optimalCost = s.OptimalCost;

            // === A* BENCHMARK ===
            // 1. Memori & Data Sekunder (Single Run)
            long memStartA = GC.GetTotalMemory(false);
            var pathA = AStar.FindPath(currentMap, s.StartX, s.StartY, s.GoalX, s.GoalY);
            long memEndA = GC.GetTotalMemory(false); // False = Jangan collect sampah hasil algo
            
            r.memoryAStar = (memEndA > memStartA) ? (memEndA - memStartA) : 0;
            r.openAStar = AStar.LastOpenList?.Length ?? 0;
            r.closedAStar = AStar.LastClosedList?.Length ?? 0;
            r.pathLengthAStar = AStar.LastFinalCost;

            // 2. Waktu (Multi Run Average)
            double timeSumA = 0;
            for(int k=0; k<iterations; k++){
                var sw = Stopwatch.StartNew();
                AStar.FindPath(currentMap, s.StartX, s.StartY, s.GoalX, s.GoalY);
                sw.Stop();
                timeSumA += sw.Elapsed.TotalMilliseconds;
            }
            r.timeAStar = timeSumA / iterations;

            // === JPS BENCHMARK ===
            // 1. Memori & Data Sekunder
            long memStartJ = GC.GetTotalMemory(false);
            var pathJ = JumpPointSearch.FindPath(currentMap, s.StartX, s.StartY, s.GoalX, s.GoalY);
            long memEndJ = GC.GetTotalMemory(false);

            r.memoryJPS = (memEndJ > memStartJ) ? (memEndJ - memStartJ) : 0;
            r.openJPS = JumpPointSearch.LastOpenList?.Length ?? 0;
            r.closedJPS = JumpPointSearch.LastClosedList?.Length ?? 0;
            // Skala JPS cost disesuaikan jika perlu (misal /10f), di sini kita ambil raw dulu
            r.pathLengthJPS = JumpPointSearch.LastFinalCost; 

            // 2. Waktu
            double timeSumJ = 0;
            for(int k=0; k<iterations; k++){
                var sw = Stopwatch.StartNew();
                JumpPointSearch.FindPath(currentMap, s.StartX, s.StartY, s.GoalX, s.GoalY);
                sw.Stop();
                timeSumJ += sw.Elapsed.TotalMilliseconds;
            }
            r.timeJPS = timeSumJ / iterations;

            results.Add(r);
        }

        // Selesai satu map
        PrintBenchmarkSummary(results);
        SaveBenchmarkToCSV(mapName, results);
        
        UnityEngine.Debug.Log($"Benchmark Completed for {mapName}");
    }

    void PrintBenchmarkSummary(List<BenchmarkResult> results)
    {
        if (results == null || results.Count == 0) return;

        double avgTimeA = results.Average(r => r.timeAStar);
        double avgMemA = results.Average(r => r.memoryAStar);
        double avgTimeJ = results.Average(r => r.timeJPS);
        double avgMemJ = results.Average(r => r.memoryJPS);

        UnityEngine.Debug.Log($"SUMMARY A*: Time={avgTimeA:F4}ms, Mem={avgMemA}b");
        UnityEngine.Debug.Log($"SUMMARY JPS: Time={avgTimeJ:F4}ms, Mem={avgMemJ}b");
        benchmarkResultsPanel.SetActive(true);
        benchmarkResultsText.text = $"SUMMARY A*: Time={avgTimeA:F4}ms, Mem={avgMemA}b\n" +
                                    $"SUMMARY JPS: Time={avgTimeJ:F4}ms, Mem={avgMemJ}b";
    }

    void SaveBenchmarkToCSV(string mapName, List<BenchmarkResult> results)
    {
        string folder = Application.dataPath + "/BenchmarkResults/";
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

        string filePath = folder + mapName + "_benchmark.csv";

        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine(
                "map,index,startX,startY,goalX,goalY," +
                "time_AStar(ms),time_JPS(ms)," +
                "open_AStar,open_JPS," +
                "closed_AStar,closed_JPS," +
                "pathLength_AStar,pathLength_JPS," +
                "optimalCost," +
                "memory_AStar(bytes),memory_JPS(bytes)"
            );

            foreach (var r in results)
            {
                writer.WriteLine(
                    $"{mapName},{r.index},{r.startX},{r.startY},{r.goalX},{r.goalY}," +
                    $"{r.timeAStar:F6},{r.timeJPS:F6}," +
                    $"{r.openAStar},{r.openJPS}," +
                    $"{r.closedAStar},{r.closedJPS}," +
                    $"{r.pathLengthAStar:F4},{r.pathLengthJPS:F4}," +
                    $"{r.optimalCost:F4}," +
                    $"{r.memoryAStar},{r.memoryJPS}"
                );
            }
        }
        UnityEngine.Debug.Log($"CSV Saved: {filePath}");
    }

    // ================== PARSE INPUT ==================
    bool ParseInput()
    {
        return int.TryParse(inputStartX.text, out startX) &&
               int.TryParse(inputStartY.text, out startY) &&
               int.TryParse(inputGoalX.text, out goalX) &&
               int.TryParse(inputGoalY.text, out goalY);
    }
}