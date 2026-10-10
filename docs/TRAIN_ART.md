# Survival tren sanatı — D47, 2026-10-09

## D50 geçerli oyun görünümü — 2026-10-10

Kullanıcının yeni isteği: **önceki tren modeli, yüzey dokusuz**. Mevcut D48 meshleri ve çalışan kapı/cam/körükler korunur; opak rendererlar düz gri/kırmızı/koyu Standard malzeme kullanır. Yeni materyallerin bütün yüzey haritaları boştur. D49 görünüm çalışması oyuna kurulmamıştır ve yeni doku fikri beklenir. Tekrar uygulama: `Wait Your Turn → Survival → Use Previous Train Without Textures` (Ctrl+Shift+Alt+U), Play durmuşken. `Install Textured Train Art` komutu eski atlası tekrar bağlar; dokusuz görünüm için onu çalıştırmayın. Uygulama kaydı `docs/generated/survival-train-plain-install.txt`.

## D49 güncel görünüm çalışması — 2026-10-10

**D48 görünümü kullanıcı tarafından reddedildi.** Aşağıdaki D48 kayıtları teknik geçmiştir; nihai sanat yönü veya görsel kabul değildir. Sıra: özgün malzeme + ışığı önce model üzerinde dene → görüntüyü değerlendir → kompakt haritalara bake et → tek vagonu Unity'de aynı kamerada doğrula → üç vagona uygula → Android yükünü ölç.

Yeni çalışma [LookDev/README](../ArtSource/SurvivalTrain/LookDev/README.md) içinde. Gerçek Blender renderi `material-lighting-preview.png`, yakın kamera `material-detail.png`, düzenlenebilir kaynak `SurvivalTrain-LookDev.blend`; özgün ImageGen yüzey atlası ve native zemin/roughness/kabartma node'ları üçüncü taraf doku gerektirmez. Kaynak atlas ve tam üretim promptu aynı klasörde tutulur. Önceki fotoğraf benzeri atlasın her küçük parçaya esneyen UV yerleşimi bu denemede kullanılmaz; desenler metre ölçeğindedir. Eşzamanlı oyun giydirme yapılmaz. Bench/bölme görselleri ayrı bir öneridir; Unity/NavMesh/collider kabulüne eklenmiş sayılmaz. Offline ışıklandırma mobil runtime maliyeti olarak sunulmaz.

## D48 görünüm ve titreme düzeltmesi — 2026-10-10

Kullanıcı oyun içi görünümü yeterli bulmadı; Blender önizlemesi nihai görünüm kabulü değildir. Körükteki titreme fizik hareketinden değil, vagon/körük zeminlerinin ve uç dikme/yan yüzeylerinin aynı düzlemde örtüşmesinden kaynaklanabilecek somut bir geometri hatası içeriyordu. Görsel körük artık yalnız 1 m boşluğu doldurur; 1,4 m fizik/nav zemini güvenli bindirmesiyle korunur. Dört birleşimin dışa bakan eksen hizalı yüzeyleri için pozitif coplanar örtüşme kontrolü mesh önkontrolüne eklendi. Ana gövde zaten tek mesh; ayrı kalan parçalar cam, portal durumunu izleyen kapı yaprakları ve iki körüktür.

36 tekrarlı parlak zemin karosu ve ince üst seam geometrisi yerine bir sürekli güverte kullanılır. Sarı/bej paneller nötr çeliğe döner; normal kabartma ve zemin parlaklığı azalır. Mevcut tek gölgesiz yönlü ışık yeniden ayarlanır, düşük ambient trilight ve statik cubemap kullanılır; ek realtime ışık/gölge/postprocess yoktur. Kamera near clip 0,1 → 0,5 m; far clip160 m kalır. Mesh yeniden kurulumunda eski GPU tamponu önce temizlenir; daha küçük mesh verisinde eski tamponun tutulup yüzeylerin yanlış çizilmesi önlenir. Mobil kabul ayrıca yapılır; görsel kalite kullanıcı oyun içi değerlendirmesiyle ilerler.

D48 kısa PC kabulü geçti: 15 kapı/cam/metal/onarım, 15 yan cam ve iki körükten fiziksel yürüyüş. Güncel oyun/körük görüntüleri `docs/generated/survival-train-game.png` ve `survival-train-bridge-rear/front.png`; yeni offline kanıt `survival-train-mesh-preflight.txt`. Kontrol sonrasında Console hata/uyarı sayısı0. Bu kısa kontrol kullanıcının bütün kamera hareketlerindeki görsel değerlendirmesinin veya Android FPS ölçümünün yerine geçmez.

## Teslim durumu

İlk sarı çizgili taslak kullanıcı tarafından reddedildi. Blokout yalnız ölçü referansıdır. Yeni tasarım kullanıcının metro görselindeki kırmızı kapı, kirli bej iç panel, koyu dış metal, basık cam ve yuvarlak kenar dilini esas alır.

**Kullanıcının önizleme sonrası isteğiyle SurvivalIntegration sahnesine kuruldu; kısa Unity Play kabulü geçti.** Blender renderi gerçek `.blend` modelinden üretildi; güncel oyun görüntüsü `docs/generated/survival-train-game.png` içindedir. İç dekor/karakter/VFX ve son ışıklandırma bu gövde tesliminin dışında kalır. D48 oyun içi görünüm düzeltmesi yukarıda kayıtlıdır; ilk önizleme onayı nihai kalite onayı sayılmaz.

| Dosya | Kullanım |
| --- | --- |
| `ArtSource/SurvivalTrain/SurvivalTrain.blend` | Düzenlenebilir, dokuları paketli kaynak |
| `ArtSource/SurvivalTrain/SurvivalTrain.fbx` | Gövde/cam/yaprak/körük parça aktarımı; Assets dışında, ikinci otomatik Blender importu yok |
| `ArtSource/SurvivalTrain/train-preview.png` | Blender renderi |
| `ArtSource/SurvivalTrain/TrainAtlas_Albedo_Source.png` | İlk D47 atlası; D48 güncel nötr çelik/sakin zemin kaynağı `TrainAtlas_Albedo_V3.png`, üretim promptları aynı klasörde |
| `Assets/Game/Art/SurvivalTrain/SourceData/TrainMeshes.json` | Yalnız Editor kurulumu okur; runtime/Resources referansı yok |

## Ölçüler ve mekanik bağ

Bir birim bir metre. Unity +X baş, +Y yukarı, +Z çapraz; Blender eşlemesi `(X, -Z, Y)`. Vagon pivotları X=0/13/26; gövde12×4,2 m, üst duvar kotu2,1 m, kapı düzenleri6/5/4. Lokomotif X=37, kapalı oynanış dışı kontrol odasıdır. Körükler X=6,5/19,5; net3 m açık geçit, karşıdan karşıya kapı/tavan/koltuk konulmaz.

Gövde, cam, hareketli kapı yaprakları ve körük ayrı meshlerdir. Modellerde yeni Rigidbody/Agent/Health veya hareket scripti yoktur. Mevcut hareket colliderları, kimlikler, can, portal, turret slotu ve NavMesh veri kaynağı korunur.

**Yeni Survival sanatının atış ölçüsü:**

| Parça | Yerel ölçü |
| --- | --- |
| Yan cam | Y=0,75…1,55; yükseklik0,8 m, eski yatay açıklıklar korunur |
| Dikey pencere metali | Ara dikme0,32 m; yan çerçeve0,42 m |
| Kapı alt yaprağı | Y=0…0,72, genişlik1,45 m |
| Kapı yan yaprakları | X=±0,66; genişlik0,13; Y=0,72…2,0 |
| Kapı üst yaprağı | Y=1,68…2,0; genişlik1,45 |
| Kapı orta metal dikişi | Genişlik0,035; Y=0,95…1,93; normal0,85–0,90 m atış çizgisinin üstünde başlar |
| Sabit kapı üst çerçevesi | Y=2,0…2,1, kırılmada gövdede kalır |

Kurucu görünen metal için atış kutularını eşler; **bu kısım yalnız kozmetik değişim değildir**. Cam rayı geçirir, metal keser. Yeni yaprak kutuları `Lower panel` altında kapıyla açılır/kapanır; NavMesh buildine katılmaz. `TrainDoorVisual` yalnız portal görünümünü takip eder. Battle sahnesi/layout'u değişmez. Kurulumdan sonra Survival ölçü CSV'si yeniden üretilir; yukarıdaki kotlar yalnız bu sanatın Survival düzeni için genel sözleşmenin eski kotlarını geçersiz kılar.

## Mobil bütçe ve doğrulama

Tam üç vagon +15 kapı +iki körük +lokomotif **24.660 üçgen**;10 benzersiz mesh, her biri UInt16 sınırının altında. Kapılar/körükler mesh paylaşır. Opak parçalar aynı atlas/materiali paylaşır; cam ayrı hafif tek pass shaderdır. Yeni gerçek ışık, realtime reflection probe, postprocess veya minimap kamerası eklenmez. Blender sunum ışıkları/zemini gameplay'e aktarılmaz.

Kurulum ayarları: albedo2048 ASTC6×6, normal1024 ASTC6×6, metal/smooth512 ASTC8×8, mipmap; CPU Read/Write kapalı. ASTC destekli cihazda üç atlasın teorik GPU karşılığı yaklaşık3,2 MB'dır; toplam uygulama belleği/build boyutu değildir. JPEG disk boyutunu düşürür; GPU tasarrufu Android import sıkıştırmasıyla sağlanır. Yardımcı normal haritası ince kabartma içindir, fotoğraf detaylarının tam normal bake'i değildir.

Blender üretimi, mesh indeks/UV/normal/yüz yönü/UInt16 önkontrolü, Unity derlemesi ve shader importu geçti. Kurulum hareket colliderı/agent/obstacle ve NavMesh kaynağının değişmediğini doğruladı. Tren geometrisinin açık renderer sayısı 243'ten 39'a indi; bu sayı toplam sahne draw call ölçümü değildir. Statik ortam cubemap'i kullanılır, realtime probe yoktur.

**Kısa PC kabulü PASS:** 15 kapıda cam/metal rayları, kırılınca görselin kalkması ve onarımla geri gelmesi; 15 yan camda açıklık ve alt/üst/dikme metal engelleri; tam tren NavMesh rotası ve oyuncunun iki açık körükten fiziksel yürümesi. Test ayrı kayıt dosyasında çalıştı. Kanıtlar `docs/generated/survival-train-art-install.txt` ve `docs/generated/survival-train-art-pc-acceptance.txt`. Console'da kontrol sonrası hata/uyarı yoktu. **A54 sanat yükü, FPS/ısınma/pil kabulü bekliyor.** Eski blokout A54 FPS sonucu bu sanata taşınmaz.

## Yeniden üretim ve kurulum

1. `python Tools/TrainArt/build_textures.py`: kaynak PNG'yi koruyarak JPEG ve yardımcı haritaları üretir.
2. `blender --background --factory-startup --python Tools/TrainArt/build_train.py`: kaynak, FBX, mesh JSON ve render üretir. `-- --no-render` renderi atlar.
3. `python Tools/TrainArt/check_meshes.py`: on parça, indeks/UV/normal/yüz yönü, UInt16 ve35k tam tren sınırı.
4. Play durdurulmuş `SurvivalIntegration`: **Wait Your Turn → Survival → Install Textured Train Art** (Ctrl+Shift+Alt+L). Battle ve yanlış vagon sayısını reddeder; yeni sanat köklerini tekrar kurabilir.
5. Kurulum hareket/nav değişmemiş kontrolünü ve `docs/generated/survival-train-art-install.txt` kaydını üretir. Kurulum yapılmadan bu kayıt/kabul var sayılmaz.
6. Play'de **Wait Your Turn → Survival → Check Train Art** (Ctrl+Shift+Alt+T), kısa izole kapı/cam/geçit kontrolünü ve oyun görüntüsünü üretir.
