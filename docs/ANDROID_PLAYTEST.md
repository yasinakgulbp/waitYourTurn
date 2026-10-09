# Güncel birleşik oyun — Android test hazırlığı

2026-10-09, M8b. Samsung A54 SM-A546E ADB'de bağlı görüldü. Bu kayıt tek başına performans kabulü değildir. Eski `NavigationLab.apk` yalnız navigasyon laboratuvarıdır.

## Build ve izolasyon

`Wait Your Turn/Run/Android/Build Integration APK`, aktif platform Android ve Play kapalıyken `Assets/Game/Scenes/TrainIntegration.unity` sahnesini `Builds/TrainIntegration.apk` olarak üretir. Development + ConnectWithProfiler açıktır. Test kimliği `com.yasinakgulbp.waityourturn.integrationtest`; mevcut lab/ana uygulama verisini değiştirmez. PlayerSettings kimlik/ad ayarları ve AAB/export bayrakları işlem sonunda geri yüklenir; build sahne listesi değiştirilmez. Ürün yayını imzası/Play AAB'si değildir. Unity sürüm kontrolüyle çakıştığından özel Ctrl+Alt+I kısayolu kaldırıldı; menü kullanılır.

## Kısa test sırası

1. Battle'da hareket, kapıdan/camdan otomatik atış, metal engeli, kısmi kapı onarımı. Başlangıç 1000 para geçici test ayarıdır.
2. Mağazadan farklı silah, turret ve dron; dokunma/yerleşim ve saldırı altında alışveriş.
3. Can, para, silah/mermi, kapı/turret ve istasyon durumu görüldükten sonra arka plana al/geri dön; kısa bekleme sonrası süreç kapat/geri aç. Son periyodik kayıttan devam beklenir, oyun kapalıyken süre ilerlemez.
4. Ölüm/sonuç sonrası yeniden açılınca bitmiş Battle dirilmemeli. Solo düğmesiyle ayrı koşu ve kayıt kontrolü. **Bu APK'daki Solo hâlâ eski rastgele vagon modudur; D37 fiziksel geçiş henüz yoktur.**
5. Kare zamanı/bellek ve hata kayıtları. `TrainIntegration` henüz açık bir `Application.targetFrameRate`/üretim kalite politikası kurmuyor; Android varsayılan tavanında görülen 30 FPS'den 60 kapasitesi sonucu çıkarılmaz. Hedef/tavan kayda geçirilir; 60 FPS hedef ölçümü ve ısı/pil/uzun oturum ayrı takip edilir.

Development build teşhis içindir; sanat sonrası yayın benzeri build ayrıca ölçülür. Bir cihaz testi bütün telefon/tabletlerin kanıtı değildir. Raporlanacaklar: hangi mod/istasyon/yük, çözünürlük/kalite/FPS hedefi, kare süresi, canlı zombi/savunma sayısı, bellek/GC, hata, arka plan/devam sonucu ve dokunma gözlemi.

## Sanat için mevcut envanter

- `Assets/BattleRoyaleHeroesPBR/Prefab/Character`: PBRDefault/PBRMaskTint karakterleri ve Animation/Animator içerikleri. Önce tek aktörü mevcut görsel çocuk köküne, collider/root motion eklemeden dene; mobil materyal/animasyon maliyeti ölçülmeden hepsini kullanma.
- `Assets/Hovl Studio/Toon projectiles/Prefabs`: projectile, hit ve flash prefabları. Ateş/isabet kozmetiği için aday; eski projectile hasar scripti ortak hitscan yerine çalıştırılmaz. Havuz ve eşzamanlı efekt sınırı uygulanacak.
- `Assets/Wagon/VagonsPrefab`: eski vagon görselleri; bugünkü MODEL_CONTRACT ölçülerine uyduğu varsayılmaz. Referans/yeniden kullanılabilir detay olarak incele.
- Kullanıcının kapsamlı satın alınmış UI paketi mevcut Assets envanterinde tanınamadı. Paket adı öğrenilince önce tek HUD/mağaza ekranında Built-in/TMP/uGUI uyumu incelenir; çekirdek ekonomi paket scriptlerine bağlanmaz.

APK derleme/kurulum/çalışma ve cihaz ölçüm sonucu aşağıya gerçek kanıt geldikçe eklenir; hazırlık tamamlandı diye test PASS yazılmaz.

### 2026-10-09 ilk cihaz kanıtı — kısmi

- Unity BuildReport `Succeeded`, 203,6 sn; APK 29.057.969 bayt. ADB `install -r` → Success; test uygulaması açıldı. Vulkan / Mali-G68 / kalite 2; ilk açılış 1080 × 2340 portre. Gerçek ekran görüntüsünde ikinci istasyon, oyuncu 96 can, turret ve oyun HUD'sı görüldü. Portre görüntüde fazla boşluk/küçük kontroller var; nihai yön/üretim UI ergonomisi kabul edilmiş değildir.
- Android gerçek dosyada envelope ve iç RunSnapshot v1 okunabildi; Battle istasyon 2/savunma ve 96/100 can kaydedilmiş. Bu otomatik yazma kanıtıdır; süreç kapatma/geri yükleme kabulü ayrıca yapılır.
- Açılışta Unity logunda `ClassNotFoundException: com.google.android.play.core.assetpacks.AssetPackManager` var. Süreç sonlanmadı, sahne/oyun ilerledi; yine de hatasız cihaz kabulü ilan edilmez. Asset Delivery/standalone APK bağımlılığı Play build aşamasından önce araştırılıp kapatılacak; gereksiz SDK veya rastgele sürüm değişimi yapılmadı. [Unity AndroidAssetPacks API](https://docs.unity3d.com/ja/current/ScriptReference/Android.AndroidAssetPacks.html) PlayCore bağımlılığını açıklar; bu belge hata kök nedeninin tek başına kanıtı değildir.
- Yeni editor build menüsü derlendi; geçici uygulama kimliği/adı geri döndü. Unity'nin yalnız build sırasında ürettiği TimeManager format değişimi bu çalışmaya dahil edilmedi. Özel kısayol çakışması giderildi; menü yolu kullanılır. FPS/ısı/pil ve iki modda tam dokunma/ölüm/uzun oturum kabulü henüz açık.

### İlk süreç kapatma / devam kontrolü

Battle üçüncü istasyon savunmasında HOME ile arka plana alındı; gerçek cihaz kaydı okundu, yalnız test paketi `am force-stop` ile kapatılıp yeniden açıldı. Yeni süreçte periyodik kayıt **aynı runId, istasyon 3 ve savunma evresi** ile devam etti. Önceki 11,17 sn'den sonraki örnekte 36,30 sn'ye ilerledi; para 1192→1208, can 96→18 (yeniden açılan gerçek savaş sırasında). Yeni koşu başlatıp parayı/canı sıfırlamıyor. Bu tek canlı Battle devam örneğidir; bütün alanların eşitlik testi, Solo, terminal ölüm ve uzun oturum kabulü değildir. Ham yerel inceleme çıktıları Builds altında Git dışında tutulur.
