# M0/M1 — navigasyon laboratuvarı

Sahne: `Assets/Game/Scenes/NavigationSandbox.unity`. Unity 6000.6.4f1, AI Navigation 2.0.14. Bu sahne kapı geçişini kanıtlar; hasar, onarım, saldırı ve istasyon akışı sonraki aşamalardır.

## Çalıştırma

Unity menüsünde **Wait Your Turn → Navigation → Open Sandbox**, ardından Play. Sahne ilk açılışta yoksa editor kurulum aracı oluşturur; var olan sahne yeniden üretilmez. Ana prototip `Assets/Scenes/SampleScene 2.unity` korunur ve build sahnesi olarak kalır.

- **Space / Open–Close:** giriş bağlantısını aç/kapat.
- **R / Reset:** zombileri dış zemine yerleştir ve kapıyı kapat.
- **T / Check 10 cycles:** tek agent ile tekrarlı kabul kontrolü.
- **1 / 10 / 30 / 60 / 100 düğmeleri:** geçici kalabalık yükleri. Son oyunun canlı sınırı değildir.

Klavye için Game görünümü odakta olmalı. Kontroller ekran düğmeleriyle de kullanılabilir. Sonuç `Logs/NavigationSandboxReport.json` dosyasına yazılır; Logs Git dışında tutulur. Ekrandaki ortalama kare süresi kaba bir gözlemdir; CPU/GPU ayrımı, profiler verisi veya Android performans raporu değildir.

## Sorumluluklar

- `EntryPortal`: link ve fiziksel engelin sahibi. Durum değişimini bildirir; geçit doluyken kapanışı bekletir. Can, saldırı, UI veya ses bilmez.
- `PortalNavigator`: NavMeshAgent hareket adaptörü. Kapalı geçitte dış yaklaşma noktasına, açık geçitte iç hedefe gider. Rota hedef/durum değişiminde yenilenir; agent geçişteyken ResetPath uygulanmaz. Yeni enemy brain bu adaptöre ayrı bağlanacak.
- `NavigationSandboxController`: geçici düğmeler, test yükleri ve kabul kontrolü. Son oyun akışı değildir. Laboratuvarda oluşturma/silme kullanır; üretim havuzu değildir.
- `NavigationSandboxBuilder`: yalnızca Editor'da sahne kurar ve iki yüzeyi bake eder. Runtime'da kapı değişince tekrar bake yapılmaz.

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

Son kod kontrolünde kalabalık testindeki ilk agentın hedefi diğerlerinin grid slotlarından biriyle çakışıyordu; kalabalık yükünde ilk agent da ayrı grid slotuna alındı. Tek agent kabul hedefi korunur. Bu son dağılımın bütün agentları hedefe ulaştırdığı henüz ölçülmedi.

İlk A54 çalıştırmasında başlangıç agentı yüzeyler yüklenmeden etkinleştiği için tek `no valid NavMesh` uyarısı verdi. Sahne/template agentı başlangıçta devre dışı tutulup `TryPlace` başarılı örnekleme sonrası etkinleştirecek şekilde düzeltildi. Laboratuvarın Android kare hedefi açıkça 60'a alındı; ilk sürümdeki 33.3 ms gözlemi varsayılan 30 FPS sınırını yansıtıyordu. Son oyunun kalite profili henüz değildir. Bu düzeltmeleri içeren dördüncü APK buildi başarılı (39.1 saniye); cihazda yeniden kabul kontrolü gerekli.

## Android hazırlığı

Hedef cihaz kullanıcı tarafından **Samsung Galaxy A54** olarak seçildi: SM-A546E, Android 16 / API 36. USB hata ayıklama izni sonrası ADB bağlantısı kuruldu; üçüncü APK kuruldu ve uygulama açıldı. İlk boş ADB listeleri bağlantı hazırlığına aitti.

Telefondaki işlevsel test geçti; kaba kalabalık ölçümü ve CPU maliyeti henüz kabul edilmedi. Dördüncü APK kurulum denemesinde cihaz ADB listesinden çıktı; bağlantı yeniden kurulması istendi. USB bağlantısı sorunu oyun hatası olarak değerlendirilmez.

İlk Android Development APK buildi **Succeeded**, 382.7 saniye; APK dosyası 27,120,228 bayt. IL2CPP/ARM64, minimum API 26; yalnızca laboratuvar sahnesi. BuildReport toplam boyutu debug çıktıları da içerdiğinden APK dosya boyutuyla aynı değildir.

Geri yükleme düzeltmesi sonrası ikinci build **Succeeded**, 18.2 saniye. Üçüncü build 39.8 saniyede, dördüncü build 39.1 saniyede başarılı. Build sonrası ürün adı, applicationIdentifier ve override bayrağı başlangıç değerleriyle aynı; build sahnesi listesi değişmedi.

Üçüncü APK uygulama başlangıcında `AndroidJavaException: ClassNotFoundException: com.google.android.play.core.assetpacks.AssetPackManager` kaydı verdi; uygulama devam etti ve geçiş testi geçti. Bu platform başlangıç hatası henüz teşhis edilmedi ve çözülmüş sayılmaz. İkinci APK'nın ayrı eski process logundaki texture hataları üçüncü APK'nın sonuçlarına karıştırılmadı.

Editor menüsü: **Wait Your Turn → Navigation → Android → Prepare Platform**; platform geçişi ve script reload bittikten sonra **Build Lab APK**. Build yalnızca laboratuvar sahnesini içerir; çıktı `Builds/NavigationLab.apk` (Git dışında). Geçici paket kimliği `com.yasinakgulbp.waityourturn.navigationlab`, Development build. Build aracı proje ürün/paket ve export ayarlarını sonunda geri yükler; prototipin build sahnesi listesine dokunmaz.

İki ayrı adım kullanılır çünkü platforma bağlı derleme sembolleri domain reload sonrası güncellenir: [Unity BuildPlayer API](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/BuildPipeline.BuildPlayer.html). USB hata ayıklama/telefon izni cihaz üzerinde kullanıcı tarafından yapılır: [Android cihaz bağlantısı](https://developer.android.com/studio/run/device).

Build aracının kimlik geri yüklemesi serialize edilmiş PlayerSettings anlık görüntüsüyle yapılır; SetApplicationIdentifier'ın değiştirdiği override bayrağı da korunur. Editor yardımcısı Unity 6000.6.4f1'e bağlıdır; singleton erişimi Unity'nin [Editor kaynak API'sinde](https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/Unsupported.bindings.cs) tanımlıdır ve oyun runtime'ına girmez. İlk buildin eski yönteminden kalan kimlik değişimi geri alındı. Android platform geçişinin otomatik normalizasyonu boş legacy ikon kayıtlarını kaldırdı; TMP fallback texture serializedVersion 3 → 4 oldu. Mevcut ikon görselleri değiştirilmedi.

## Açık işler

- A54 üzerinde son APK ile başlangıç NavMesh düzeltmesini ve 10/10 kontrolünü tekrarlamak; 60 FPS hedefinde kapalı kuyruk/açık geçiş yüklerini ölçmek. Navigation, physics ve sunum maliyetini Profiler ile ayırmak. Bu yapılmadan M1 bütünü tamamlanmış sayılmaz.
- Android AssetPackManager başlangıç hatasını teşhis etmek; başarılı geçiş kontrolü bunu gidermez.
- Gerçek kapı canı M2'de, kırılma/onarım ve saldırı kararları M3'te bağlanır. Laboratuvar düğmesi kapının son oyundaki davranışı değildir.
- Kuyruk offsetleri test içindir; saldırı slotu rezervasyonu ve rota hesaplama bütçesi kalabalık enemy sistemiyle geliştirilir. NavMesh avoidance tek başına saldırı slotu yönetmez.
- Minimum mobil kontrol/tabanca mermi kuralı kullanıcıyla netleştirilecek; bu sahne bunları varsaymaz.

## Referans prototip kontrolü

Altı zombi prefabından yalnızca AudioListener bileşenleri kaldırıldı. `SampleScene 2` Play denemesinde zombi üretimi/saldırısı gözlendi; kontrol edilen yeni log bölümünde çoklu AudioListener uyarısı ve Exception yoktu. Ayrıca `Attempted to call .Dispose on an already disposed CancellationTokenSource` mesajı görüldü; bu eski sahnenin tamamen hatasız olduğuna dair bir kabul değildir. Düzeltme ayrı `973a8ff` commitinde kayıtlıdır.
