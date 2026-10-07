# M0/M1 — navigasyon laboratuvarı

Sahne: `Assets/Game/Scenes/NavigationSandbox.unity`. Unity 6000.6.4f1, AI Navigation 2.0.14. Bu sahne kapı geçişini kanıtlar; hasar, onarım, saldırı ve istasyon akışı sonraki aşamalardır.

## Güncel düzen — kullanıcının geometri/giriş gözlemi sonrası

Kapı artık vagon ön duvarında **z=1** düzleminde ve vagonun alt nesnesidir. İstasyonun kenarı z=0.95, vagon zemini z=1; aralık **0.05 m**. Yalnızca 0.1 m uzunluğunda kapı eşiği vardır; uzak kapı ve geniş boarding bridge kaldırıldı. İki sabit geçiş linki yerine ortak bake edilmiş `ConnectedNavMesh.asset` ve kapı açıklığında `NavMeshObstacle.carving` kullanılır. Kapı açılınca normal yürüyüş alanı oluşur; sabit birer/ikişer geçiş kilidi yoktur. Bake yalnızca Editor'da, carving ise kapı değiştiğinde uygulanır.

`EntryPortal` eşikteki gövdeler çıkmadan collider/nav kesitini kapatmaz. Eşik içindeki agent kapanış isteğinde mevcut yönünde yürür; diğerleri dış hedefe döner. Carving etkisi aynı çağrıda oluşmaz; kabul kontrolü navigation güncellemesini bekler. Referans: [Unity NavMeshObstacle](https://docs.unity3d.com/cn/2018.3/Manual/class-NavMeshObstacle.html). Kapanış beklerken fizik sorgusu öncesinde transformlar eşitlenir; sürekli açık/kapalı durumda bu sorgu çalışmaz.

Kullanıcı talebiyle rutin kontrol **30 agent / bilgisayar** oldu. Yeni mobil build hazırlanmadı; aşağıdaki A54 kanıtları önceki link geometrisine aittir. Önemli mekanik/geometri veya birleşik oynanış değişiminde cihaz kontrolü yeniden yapılacak. Test agentlarında küçük hız/avoidance priority farkları vardır; yürüyüş animasyonu ve gerçek saldırı hedefleri M3'tedir.

İlk kesintisiz yol denemesi: **29/30**, 120 saniyede zaman aşımı (`2026-10-07T16:58:27.9279938Z`), tepe eşik bölgesi sayıları 5/4. Bu başarılı sonuç diye yazılmaz. Duran test hedeflerinin geçişi tıkamaması için 30 agent yerleşimi daha seyrek 5×6 düzene geçirildi (x aralığı 1.3 m, z aralığı 1.1 m). Test gridinin doluluğu gerçek saldırı-slotu davranışı değildir.

Seyrek düzen de **29/30 hedef varışı**, 120 saniye (`2026-10-07T17:10:11.5213932Z`) verdi; eşik bölgesi tepe sayıları 6/5. `Lab Zombie 22` vagon içinde (-2.828, 0.050, 4.150) konumunda, (-1.300, 0, 3.600) hedefine 1.625 m kala diğer duran agentların avoidance etkisinde bekledi; rota `PathComplete`. Kapıdan giriş ile vagon içinde hedefe yerleşme ayrı sonuçlardır. Kalabalık hedef seçimi/yer açma M3/M6'da ele alınacak; bu başarısız hedef yerleşimi kaydı korunur. Eşik sayaçları bölgedeki gövdeleri sayar; 5 zombinin yan yana geçtiği anlamına gelmez.

Yeni ortak yüzey/carving geometrisinde **10/10 PASS**, WindowsEditor (`2026-10-07T17:15:41.8290546Z`): kapalı/açık rotalar, fiziksel eşikten geçiş, dolu eşikte güvenli kapanış, geri dönüş ve geçersiz doğuş reddi. Bu tek-agent kanıtı, yukarıdaki kalabalık hedef yerleşimi eksikliğini kapatmaz.

## Çalıştırma

Unity menüsünde **Wait Your Turn → Navigation → Open Sandbox**, ardından Play. Sahne ilk açılışta yoksa editor kurulum aracı oluşturur; var olan sahne yeniden üretilmez. Ana prototip `Assets/Scenes/SampleScene 2.unity` korunur ve build sahnesi olarak kalır.

- **Space / Open–Close:** giriş bağlantısını aç/kapat.
- **R / Reset:** zombileri dış zemine yerleştir ve kapıyı kapat.
- **T / Check 10 cycles:** tek agent ile tekrarlı kabul kontrolü.
- **C / Check 30 agents:** 30 agentta kapalı rota, geçiş sırasında güvenli kapanış, yeniden açılma ve her ayrı hedefe varış kontrolü. Varış sınırı 120 saniye, hedef mesafesi ≤0.4 m; başarısız agentların konum/rota bilgisi loglanır.
- **1 / 10 / 30 / 60 / 100 düğmeleri:** geçici kalabalık yükleri. Son oyunun canlı sınırı değildir.

Klavye için Game görünümü odakta olmalı. Kontroller ekran düğmeleriyle de kullanılabilir. Editor sonuçları `Logs/NavigationSandboxReport.json` ve `Logs/NavigationCrowdReport.json` dosyalarına, cihaz sonucu ADB uygulama loguna yazılır; Logs Git dışında tutulur. Kod değiştirirken Play durdurulur; derleme/domain reload sonrası kalabalık testi temiz Play oturumundan başlatılır. Ekrandaki ortalama kare süresi kaba bir gözlemdir; CPU/GPU ayrımı, profiler verisi veya Android performans raporu değildir.

## Sorumluluklar

- `EntryPortal`: kapının nav kesiti ve fiziksel engelin sahibi. Eşik doluyken kapanışı bekletir. Can, saldırı, UI veya ses bilmez.
- `PortalNavigator`: NavMeshAgent hareket adaptörü. Kapalı geçitte dış yaklaşma noktasına, açık geçitte iç hedefe gider. Rota hedef/durum değişiminde yenilenir; agent geçişteyken ResetPath uygulanmaz. Yeni enemy brain bu adaptöre ayrı bağlanacak.
- `NavigationSandboxController`: geçici düğmeler, test yükleri ve kabul kontrolü. Son oyun akışı değildir. Laboratuvarda oluşturma/silme kullanır; üretim havuzu değildir.
- `NavigationSandboxBuilder`: yalnızca Editor'da sahne kurar ve ortak yüzeyi bake eder. Runtime'da kapı değişince tekrar bake yapılmaz. `Upgrade Lab Platform Edge` labı güncel geometriyle yeniden kurar; yalnızca Edit modunda kullanılır.

İstasyon/vagon collision ve NavMesh kökleri sabittir. Hareket eden görsel çevre kökünde collider veya nav yüzeyi yoktur. Zombi kökünü yalnızca NavMeshAgent sürer; Rigidbody yoktur, görsel root motion kapalıdır. Eski zombi modeli kullanılır; eski hareket/hasar scriptleri laboratuvara alınmaz.

Üç küçük assembly sınırı kuruldu: Navigation → Unity.AI.Navigation; Sandbox → Navigation; Editor kurulum aracı → bu ikisi ve Unity.AI.Navigation. Navigation, laboratuvar UI'ına veya eski prototip scriptlerine bağımlı değildir. Assembly referansları açık tanımlıdır; yeni sistemler ihtiyaç oldukça eklenecek.

`IsInside` bu denemede portal düzleminin hangi tarafında olunduğunu gösterir. Tam vagon üyeliği/OnBoard kararı değildir; bu kayıt M4a'da giriş bölgesi ve kalkış kuralıyla kurulacak. `TryPlace` doğru agent tipi/alan filtresiyle yakın NavMesh konumu arar; geçersiz yerde instantiate edip hatayı sonradan düzeltmez.

## Tekrarlı kontrol

Her döngü şu davranışları denetler:

1. Kapalı bağlantıda içerideki hedefe tam rota oluşmaz; zombi dış noktaya gelir.
2. NavMesh dışında verilen doğuş noktası reddedilir ve agent taşınmaz.
3. Bağlantı açılınca rota tamamlanır ve gerçek link geçişi başlar.
4. Geçiş sırasında kapanış isteği hemen collider açmaz; geçiş tamamlandıktan sonra kapanır.
5. Yeniden açılınca ters yönde dönüş çalışır.

2026-10-07 Play sonucu: **10/10 PASS**, WindowsEditor; son modül ayrımı ve agent tipi filtresi sonrası tekrarın rapor UTC'si `2026-10-07T14:40:39.0798352Z`. A54 üzerinde üçüncü APK ile aynı kontrol **10/10 PASS** (`2026-10-07T15:21:47Z`, ADB uygulama logu). Bu sonuçlar tek geçit/tek agent geometrisine aittir. Çok kapı, farklı agent boyları, oyuncunun geçitte durması ve kalkış üyeliği henüz kanıtlanmadı.

10/30/60/100 agent ile elle kapalı kuyruk ve açık giriş gözlendi. Ekrandaki kısa ortalama örnekleri editörde yaklaşık 3–5.2 ms aralığındaydı; düzenli süreyle alınmış benchmark veya mobil hedef kabulü değildir. 60/100 yükünde dar kapıda kuyruk/sıkışıklık görüldü; bütün agentların hedefe erişme süresi bu denemede ölçülmedi. İç hedeflerin laboratuvar dağılımı daha geniş aralığa taşındı; üretim saldırı slotu sistemi henüz yok.

Son kod kontrolünde kalabalık testindeki ilk agentın hedefi diğerlerinin grid slotlarından biriyle çakışıyordu; kalabalık yükünde ilk agent da ayrı grid slotuna alındı. Tek agent kabul hedefi korunur. Sonraki programatik varış kontrolleri aşağıda kayıtlıdır.

İlk A54 çalıştırmasında başlangıç agentı yüzeyler yüklenmeden etkinleştiği için tek `no valid NavMesh` uyarısı verdi. Sahne/template agentı başlangıçta devre dışı tutulup `TryPlace` başarılı örnekleme sonrası etkinleştirecek şekilde düzeltildi. Laboratuvarın Android kare hedefi açıkça 60'a alındı; ilk sürümdeki 33.3 ms gözlemi varsayılan 30 FPS sınırını yansıtıyordu. Son oyunun kalite profili henüz değildir. Bu düzeltmeleri içeren dördüncü APK buildi başarılı (39.1 saniye); cihazda **10/10 PASS** tekrarlandı (`2026-10-07T15:39:39.095Z`). Yeni process başlangıç logunda `no valid NavMesh` görülmedi.

## A54 kaba kalabalık ölçümü — 2026-10-07

Ölçüm APK (dördüncü build): 27,124,190 bayt; Development / IL2CPP / ARM64; kalite indeksi 2 (Medium), 1080×2340 portre, hedef 60 FPS. Profiler bağlı, Deep Profile kapalı. Her yük düğmesinden 8 saniye sonra kapalı kuyruk, ardından kapı açılışından 8 saniye sonra açık geçiş görüntüsü alındı. Aşağıdaki değerler bu görüntülerdeki son bir saniyelik ortalamalardır; sekiz saniyenin ortalaması veya p95 değildir.

| Agent | Kapalı kuyruk | Açık geçiş |
| --- | --- | --- |
| 10 | 16.7 ms (~60 FPS) | 16.7 ms (~60 FPS) |
| 30 | 16.7 ms (~60 FPS) | 16.7 ms (~60 FPS) |
| 60 | 16.7 ms (~60 FPS) | 16.7 ms (~60 FPS) |
| 100 | 16.7 ms (~60 FPS) | 16.7 ms (~60 FPS) |

Yerel kanıtlar: `Logs/A54-{10,30,60,100}-{closed,open}.png`. Yük üretme anlarının sıçramaları bu tabloya dahil değildir. Test prefabının animasyonu/saldırısı/VFX'i yoktur; sonuç tam oyunda 100 zombinin 60 FPS çalışacağı garantisi değildir.

100 agent açık geçit sonrasındaki Profiler örneği, frame **16932**: Main Thread 7.56 ms; `PreUpdate.AIUpdate` 1.11 ms (`NavMeshManager` 1.08 ms), `FixedUpdate.PhysicsFixedUpdate` 0.88 ms, `Update.ScriptRunBehaviourUpdate` 0.23 ms, renderer güncelleme 0.10 ms. `FinishFrameRendering` 4.44 ms; bu marker bekleme/sunum da içerebildiğinden saf GPU maliyeti diye yorumlanmaz. GPU zamanı bu capture'da yok. Bunlar tek kare değerleridir, istatistiksel bütçe değildir. Laboratuvar sunumunda yaklaşık 10.8 KB/kare GC allocation görülür; üretim UI'ı bu IMGUI kontrolünden alınmayacak. Ham capture yerelde `ProfilerCaptures/The Gates Are Opening - Harmony_2026-10-07_18-43-03.data` olarak doğrulandı (189,031,968 bayt); üretilen capture klasörü Git dışında tutulur.

30–100 yükünde 8 saniyelik açık örnekte dışarıda hâlâ kuyruk vardı. Daha sonraki `Logs/A54-100-later.png` görüntüsünde kalabalık vagon içine taşınmıştı; kalıcı deadlock kanıtı yok. Tüm agentların ayrı hedefe erişme süresi/sayısı programatik ölçülmedi. Üretimde geçiş sırası ve saldırı slotu rezervasyonu M3/M6'da ele alınır; NavMesh avoidance bunu tek başına çözmez. 100 agent sonraki tek bellek örneği: PSS 317,405 KiB (~310 MiB), RSS 416,782 KiB (~407 MiB). Bu tek snapshot bellek sızıntısı testi değildir.

## Önceki link geometrisinin kontrol kaydı

1.2 m bağlantı / z=1.8 çıkışla temiz Editor Play testleri: ilk deneme **99/100**, 120 saniyede zaman aşımı (`2026-10-07T16:01:55.3568506Z`); sonraki deneme **100/100 PASS**, 64.78 saniye (`2026-10-07T16:08:29.7679784Z`). İki sonuç da korunur; tek başarılı koşu kararlılık garantisi değildir. Her agentın ayrı hedefine ≤0.4 m yaklaşması istenir; yalnızca vagon tarafına geçmiş olması yeterli sayılmaz.

Kullanıcının eşzamanlı giriş isteği üzerine ilk denemede tek link **1.6 m** genişliğe ve **z=2.2** çıkışa alındı. Beşinci APK A54 sonucu: **99/100**, 120 saniye; tepe link/kapı doluluğu **1/1** (`2026-10-07T16:23:22.5176770Z`). Son agent kapıyı geçmişti: konum (3.333, 0.05, 6.852), hedef (2.925, 0, 7.85), tam rota, yaklaşık sıfır hız. Bu, kapı kilitlenmesi ile iç hedefte avoidance sıkışmasını ayırır. Tek linki genişletmek eşzamanlı geçiş sağlamadı.

Unity [IsLinkOccupied API](https://docs.unity3d.com/kr/current/ScriptReference/AI.NavMesh.IsLinkOccupied.html) her link instance'ının aynı anda tek agent tarafından geçildiğini belirtir. Son düzenlemede aynı kapının içinde **iki paralel link** kullanılır: x=−0.45/+0.45 m, her birinin genişliği 0.2 m; başlangıç z=−1.7, çıkış z=2.2. Fiziksel açıklık 2.4 m, köprü 2.2 m, agent yarıçapı 0.3 m; colliderlar ve bake edilmiş yüzeyler değiştirilmedi. Hatların uçları kapı açıklığı içinde kalır. Editor menüsündeki `Apply Lab Parallel Passages` mevcut lab sahnesini bu değerlere getirir; yeni sahne kurucusu aynı değerleri kullanır.

Lab varış gridinin aralığı 0.65 → 0.7 m oldu ve hedefler arkadan öne sıralandı (z=8.3 → 2.0). Önce gelen test agentlarının kapı önünde durup sonrakilerin hedef yolunu kapatması azaltılır. Bu yalnızca test yerleşimidir; gerçek oyunun düşman hedef seçimi veya saldırı slotu uygulaması değildir.

Altıncı APK A54 sonucu: **100/100 PASS**, 36.25 saniye; tepe link/kapı doluluğu **2/2** (`2026-10-07T16:30:41.9610570Z`). Kapalı rota, dolu geçidin güvenli kapanışı, yeniden açılma ve tüm ayrı hedeflere erişim kontrol edildi. Bağlantılar iki agentın aynı anda geçmesine izin verdi; ekrandaki sonuç örneği 16.7 ms (~60 FPS). Kanıt: `Logs/A54-parallel-crowd-pass.png` ve uygulama logu. Süre, güvenli kapanıştan sonra tekrar açılan kapıdan tüm agentların hedeflerine varışına kadardır; önceki farklı grid/geometri koşusuyla saf hız karşılaştırması yapılmaz.

Son iki hatlı APK üzerinde tek agent kontrolü de **10/10 PASS** olarak tekrarlandı (ADB uygulama logu, 2026-10-07 yerel saat 19:33:13). Kapalı/açık yollar, gerçek geçiş, doluyken güvenli kapanma, ters yönde dönüş ve geçersiz spawn kontrol edildi. Bu sonuçlarla M1 laboratuvar kabulü tamamlandı; üretim oyununa entegrasyon M3'te yapılacak.

Kalabalık raporuna linkteki tepe agent sayısı ve kapı düzlemindeki ±0.3 m bandın tepe doluluğu eklendi. İkincisi yalnızca uzun bağlantıda peş peşe yürüyenleri eşzamanlı kapı girişinden ayırmaya yardımcı olur; fiziksel çarpışma veya animasyon gerçekçiliği garantisi değildir. Üretim saldırı slotları, değişken hız/animasyon ve doğal kalabalık davranışı bu küçük geometri düzeltmesinin kapsamı değildir.

## Android hazırlığı

Hedef cihaz kullanıcı tarafından **Samsung Galaxy A54** olarak seçildi: SM-A546E, Android 16 / API 36. USB hata ayıklama izni sonrası ADB bağlantısı kuruldu; üçüncü APK kuruldu ve uygulama açıldı. İlk boş ADB listeleri bağlantı hazırlığına aitti.

Telefondaki işlevsel test geçti. Dördüncü APK kurulum denemesinde cihaz ADB listesinden çıktı; USB hata ayıklama yeniden açılınca bağlantı geri geldi ve son APK kuruldu. USB bağlantısı sorunu oyun hatası olarak değerlendirilmez. Profiler otomatik bağlantısı dördüncü APK'da doğrulandı.

İlk Android Development APK buildi **Succeeded**, 382.7 saniye; APK dosyası 27,120,228 bayt. IL2CPP/ARM64, minimum API 26; yalnızca laboratuvar sahnesi. BuildReport toplam boyutu debug çıktıları da içerdiğinden APK dosya boyutuyla aynı değildir.

Geri yükleme düzeltmesi sonrası ikinci build **Succeeded**, 18.2 saniye. Üçüncü build 39.8 saniyede, dördüncü build 39.1 saniyede başarılı. Build sonrası ürün adı, applicationIdentifier ve override bayrağı başlangıç değerleriyle aynı; build sahnesi listesi değişmedi.

Kalabalık sayaçlarıyla beşinci build 53.9 saniyede; iki paralel hatla altıncı build 36.4 saniyede başarılı. Son APK 41,739,426 bayt ve A54 üzerinde kurulum/çalıştırma doğrulandı. Unity, SceneTemplateSettings içine güncel `UnityEngine.PhysicsMaterial` tip kaydını otomatik ekledi; gameplay ayarı değildir.

Üçüncü/dördüncü APK uygulama başlangıcında `java.lang.ClassNotFoundException: com.google.android.play.core.assetpacks.AssetPackManager` kaydı verdi; uygulama devam etti ve geçiş testi geçti. Yerel Unity 6000.6.4f1 Android Player `Variations/il2cpp/Development/Classes/classes.jar` içindeki `PlayAssetDeliveryUnityWrapper` bytecode'u `javap -c -p` ile incelendi: constructor isteğe bağlı PAD sınıfını arıyor, ClassNotFoundException'ı yakalayıp logError ile yazıyor ve geri dönüyor. Bu kayıt yeni C# navigasyon scriptlerinin exception'ı veya mevcut labın çökmesi değildir. PAD kullanılacak yayın buildinde ilgili teslimat bağımlılıkları ayrıca doğrulanmalı; yalnızca logu susturmak için lab APK'sına paket eklenmedi. İkinci APK'nın ayrı eski process logundaki texture hataları üçüncü APK'nın sonuçlarına karıştırılmadı.

Editor menüsü: **Wait Your Turn → Navigation → Android → Prepare Platform**; platform geçişi ve script reload bittikten sonra **Build Lab APK**. Build yalnızca laboratuvar sahnesini içerir; çıktı `Builds/NavigationLab.apk` (Git dışında). Geçici paket kimliği `com.yasinakgulbp.waityourturn.navigationlab`, Development build. Build aracı proje ürün/paket ve export ayarlarını sonunda geri yükler; prototipin build sahnesi listesine dokunmaz.

İki ayrı adım kullanılır çünkü platforma bağlı derleme sembolleri domain reload sonrası güncellenir: [Unity BuildPlayer API](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/BuildPipeline.BuildPlayer.html). USB hata ayıklama/telefon izni cihaz üzerinde kullanıcı tarafından yapılır: [Android cihaz bağlantısı](https://developer.android.com/studio/run/device).

Build aracının kimlik geri yüklemesi serialize edilmiş PlayerSettings anlık görüntüsüyle yapılır; SetApplicationIdentifier'ın değiştirdiği override bayrağı da korunur. Editor yardımcısı Unity 6000.6.4f1'e bağlıdır; singleton erişimi Unity'nin [Editor kaynak API'sinde](https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/Unsupported.bindings.cs) tanımlıdır ve oyun runtime'ına girmez. İlk buildin eski yönteminden kalan kimlik değişimi geri alındı. Android platform geçişinin otomatik normalizasyonu boş legacy ikon kayıtlarını kaldırdı; TMP fallback texture serializedVersion 3 → 4 oldu. Mevcut ikon görselleri değiştirilmedi.

## Açık işler

- M1 lab kabulü tamamlandı: kaba A54 ölçümü, iki eşzamanlı giriş, kalabalık varış ve güvenli kapanma/yeniden açılma kanıtlandı. Farklı agent boyları, oyuncunun eşikte durması ve daha doğal animasyon/hız davranışı M3/M6 entegrasyonunda ayrıca doğrulanacak. Uzun süreli ısı/bellek ve tam oyun maliyeti sonraki performans kontrolleridir.
- Yayın buildinde PAD kullanılacaksa Unity/Google Play teslimat bağımlılıklarını doğrulamak; mevcut lab yalnızca yerel APK'dır.
- Gerçek kapı canı M2'de, kırılma/onarım ve saldırı kararları M3'te bağlanır. Laboratuvar düğmesi kapının son oyundaki davranışı değildir.
- Kuyruk offsetleri test içindir; saldırı slotu rezervasyonu ve rota hesaplama bütçesi kalabalık enemy sistemiyle geliştirilir. NavMesh avoidance tek başına saldırı slotu yönetmez.
- Minimum mobil kontrol/tabanca mermi kuralı kullanıcıyla netleştirilecek; bu sahne bunları varsaymaz.

## Referans prototip kontrolü

Altı zombi prefabından yalnızca AudioListener bileşenleri kaldırıldı. `SampleScene 2` Play denemesinde zombi üretimi/saldırısı gözlendi; kontrol edilen yeni log bölümünde çoklu AudioListener uyarısı ve Exception yoktu. Ayrıca `Attempted to call .Dispose on an already disposed CancellationTokenSource` mesajı görüldü; bu eski sahnenin tamamen hatasız olduğuna dair bir kabul değildir. Düzeltme ayrı `973a8ff` commitinde kayıtlıdır.
