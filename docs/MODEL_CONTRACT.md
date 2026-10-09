# Model üretimi ve oyuna bağlama sözleşmesi

**D39 — 2026-10-09 Solo bağlantısı:** 12 × 4,2 m dış gövde ve yan kapı/cam ölçüleri korunur. Komşu vagon uçlarında merkezde geçit, 1 m boşlukta sabit zemin ve ayrı manuel kapı vardır. Battle'da bu kapılar kapalıdır; Solo'da satın alınıp açılır. İç kapı dış savunma kapısı değildir. Güncel [geçit ölçü CSV'si](generated/solo-passage-dimensions.csv) aşağıdaki tabloyla birlikte kullanılır; PC kabulü cihaz kabulünün yerine geçmez. İlk tek-vagon sanat örneği bu ölçülerle hazırlanabilir, bütün uç modellerinin üretimi kullanıcı görünüm kabulünden sonra genişletilir.

## Solo ara bağlantı ölçüleri

Bağlantı pivotu iki vagonun tam ortasında, Y=0/Z=0; dünya X=6,5/19,5/32,5/45,5. Yerel X tren boyuna eksenidir. Tüm ölçüler metre, tam boyuttur.

| Parça | Yerel merkez | Boyut / kural |
| --- | --- | --- |
| Uç dikmeler (vagon pivotuna göre) | X=±5,9; Y=1,05; Z=±1,4 | 0,2 × 2,1 × 1,4; orta net açıklık 1,4 m. Metal atış/beden engeli. |
| Uç üst metal (vagon pivotuna göre) | X=±5,9; Y=2,175; Z=0 | 0,2 × 0,15 × 1,4; alt kot 2,1 m, dron küresi için açık yükseklik. |
| Bağlantı zemini | (0; −0,12; 0) | 1,4 × 0,24 × 1,6; üst kot 0, uçlarla 0,2 m bindirme. Sabit NavMesh kaynağı. |
| Bağlantı yan metal | (0; 0,55; ±0,8) | 1,4 × 1,1 × 0,2; net iç genişlik 1,4 m. |
| Manuel kapı | (0; 1; 0) | 0,16 × 2 × 1,4; kapalıyken metal atış/beden engeli ve carving. Açılınca mekanik engel devre dışıdır. |
| İzinli geçiş hacmi | (0; 1; 0) | 3,4 × 2 × 1,4; açık komşu odalarla bindirir. Sanat boyutu veya bütün tren hareket sınırı değildir. |

`Replaceable gate visual` ayrı görsel çocuktur. Model kapı açılma hareketini görselde uygular; kapı durumunun, collider/carving'in ve hareket hacminin sahibi `InteriorConnection`/`SoloProgression` kalır. Yeni görsel collider, root motion veya ikinci NavMeshSurface taşımaz. Uç dikme/üst metal ve bağlantı zemini kutularının rendererları sanatla değişebilir; fizik kökleri ve boyutları korunur. Dron uçuş kökü 1,95 m / küre 0,14 m olduğundan üst metali aşağı sarkıtmayın. Geçitte insan/zombi/dron varken kapanış reddedilir.

2026-10-08 — D23 ölçüleri, D24 ateş edilebilir yan pencereler. Nihai sanat çalışmasına başlamadan modelci bu belgeyi, [görsel referansları](VISUAL_SCALE.md) ve sahneden üretilen [ölçü listesini](generated/train-model-dimensions.csv) birlikte kullanır. Yeni nesne/mekanik ölçüsü kararlaştırılırsa burada ve tanım varlığında güncellenir; sadece render modelini büyüterek fizik düzeltilmez.

## Birimler, pivot ve yönler

- Unity'de 1 birim = 1 metre kabul edilir. Model dosyası Unity'ye alındığında kök rotation=(0,0,0), scale=(1,1,1) ve aşağıdaki boyutlarla uyuşmalıdır; DCC uygulamasındaki eksen dönüşümü export/import sırasında çözülür.
- Vagon pivotu iç zeminin merkezinde, Y=0'dadır. X uzunlamasına tren ekseni, Y yukarı, Z enine eksendir. Kuzey +Z, güney −Z'dir. X sağ/sol, görüntüdeki yatay düzene karşılık gelir.
- Oyuncu/zombi pivotu ayakların zeminle temasında, Y=0; ileri yön +Z. Normal beden yüksekliği 1,7, genişlik/derinlik 0,6. Capsule/CharacterController/NavMeshAgent kökü korunur; model ayrı görsel çocuğa eklenir.
- Vagon dünya konumlarını modele gömme: her vagon yerel merkezinde üretilir. Mevcut beşli dizinin merkezleri X=0/13/26/39/52; bu sahne yerleşimidir, mesh pivotu değildir.

## Sabit temel ölçüler

| Eleman | Yerel konum / boyut |
| --- | --- |
| İç zemin | X=−6…+6, Z=−2,1…+2,1, üst yüzey Y=0 |
| Tam yan duvar | Z=±2,1; Y=0…2,1; fizik kalınlığı 0,18 |
| Uç duvar | X=±5,9; kalınlık 0,2; Y=0…2,1 |
| Kapı slotları | X=−3,8 / 0 / +3,8, Z=±2,1; gövdeden ayrı kapı parçaları |
| Kapı geçiş genişliği | 1,45; hareket engeli Y=0…2,2 |
| Kapı alt metal paneli | Y=0…0,6; mermiyi engeller, kapıyla birlikte açılır |
| Kapı üst camı | Y=0,6…2,0; mermi geçer, sağlam kapıdan beden geçmez |
| Kapı dış yan dikmeleri | Açıklığın dış kenarına bitişik, genişlik 0,07; tam duvar yüksekliğinde |
| Kapı üst metal çerçevesi | Y=2,0…2,1; mermiyi engeller |
| Yan pencere net camı | Y=0,7…1,9; net yükseklik 1,2; aşağıdaki X aralıkları |
| Pencere alt/üst metali | Y=0…0,7 / 1,9…2,1; mermiyi engeller |
| Duvar segmenti dış pencere çerçevesi | X'te 0,42 genişlik; tam duvar yüksekliğinde |
| Aynı segmentte iki camı ayıran dikme | X'te 0,32 genişlik; tam duvar yüksekliğinde |
| Vagonlar arası mesafe | Gövde uçları arasında 1,0 |

Atış çekirdeğinin namlusu aktör kökünden Y=0,9, hedef merkezi Y=0,85'tir. Modeldeki namlu/efekt bağlantısı bu atış çizgisine uymalıdır; modelin silahını başka yüksekliğe koymak mekanik namluyu kendiliğinden değiştirmez. Büyük düşmanlar veya yüksek turret namluları için aynı tablo ve kabul kontrolleri ayrıca değerlendirilir.

## Pencerelerin X aralıkları

Her iki tarafta yalnız dış kapılar **arasında** kalan duvar segmentlerine pencere açılır. Vagon uçları ile dış kapılar arasındaki dört köşe tamamen metaldir; hiç bir yerleşimde burada cam veya atış açıklığı bulunmaz. Segmentin iki ucundan 0,42 metal ayrılır. Kalan alan, net genişliği en fazla 2,2 olan eşit camlara ve aralarında 0,32 metal dikmeye bölünür.

| Segment | Net cam merkez X / genişlik |
| --- | --- |
| Sol uç (−6…−4,525) | Tamamen metal, pencere yok |
| Sağ uç (+4,525…+6) | Tamamen metal, pencere yok |
| Orta kapı mevcutken sol ara duvar (−3,075…−0,725) | −1,9 / 1,51 |
| Orta kapı mevcutken sağ ara duvar (+0,725…+3,075) | +1,9 / 1,51 |
| Orta kapı yokken büyük ara duvar (−3,075…+3,075) | −1,876667 / 1,556667; 0 / 1,556667; +1,876667 / 1,556667 |

SixDoor iki tarafta orta kapı taşır (4 yan pencere). FiveDoor kuzey orta kapıyı kaldırır (5 pencere). FourDoor iki orta kapıyı kaldırır (6 pencere). Beşli sahnede toplam 26 yan pencere vardır. Kapı sayısı zombi giriş sayısıdır; pencere ayrı bir zombi girişi değildir. Pencere kırılması, canı veya onarımı bu kararda yoktur.

## Kesit görünümü ve bağlama

2026-10-08 kullanıcı düzeltmesi: **bütün yan/uç duvarların görünür üst kotu Y=2,1**. Ön duvarı 1,15'te, uçları 1,6'da kesen eski görünüm kaldırıldı. Cam iki yanda aynı açıklıktadır; üst metal ince (0,2), dikey metal daha geniştir. Kapı üst çerçevesi Y=2,0…2,1 sabit gövde parçasıdır; kapı kırıldığında yalnız alt panel ve kapı camı gizlenir, üst çerçeve/dikmeler kalır. Modelci bu sabit parçaları hareketli kapı meshine birleştirmez. Tavan oynanabilir vagonlarda açık kesit, lokomotifte kapalıdır.

`Replaceable visuals - no gameplay ownership` ve `Replaceable actor visuals - no colliders` değiştirilebilir sanat kökleridir. Camın net sınırı ile fizik açıklığı aynı olmalıdır; metal çerçeve/dikme görünümü karşılık gelen kutuyla örtüşür. Render modelinde collider, NavMeshAgent, ayrı hareket/hasar scripti veya root motion etkinleştirilmez. Kapı açılma görünümü mevcut `DoorPanels`/portal durumuna bağlanır; hasar ve yol açma işi model animasyonuna verilmez.

Cam malzemesi mermi kuralını belirlemez. Tam yan sınır `ShotTransparent` katmanında beden/NavMesh engelidir; ayrı metal kutular normal atış katmanındadır. Cam görseli collider taşımaz. Ortak `HitscanResolver` ve silah maskesi cam sınırını atlar, en yakın metal/bedene çarpar. Metal model tek parça opak colliderla bütün pencereyi kapatmamalıdır. Pompalının her saçması ayrı raydır; ince dikmenin yanından geçen saçma hedefi vurabilir, dikmeye çarpan saçma orada durur.

## Üretilen ölçü listesi ve teslim kontrolü

Blok sahnenin cam malzemesi %28 opaklıkta sade renkli camdır; bu sadece okunurluk içindir. Alpha/ışık ayarını değiştirmek atış açıklığını değiştirmez. Nihai cam shaderı/ışıklar ayrı sanat ve cihaz bütçesi değerlendirmesidir. CSV ölçüleri 0,001 birime yuvarlanır; yukarıdaki tablo ilk düzenin daha hassas uç merkezlerini de verir.

**Build Five Wagons**, `WagonLayoutDefinition` verileriyle sahneyi ve NavMesh'i yeniden kurar; ardından `docs/generated/train-model-dimensions.csv` dosyasını günceller. Bu dosya üretim çıktısıdır, elle düzeltilmez. Sayılar noktalı ondalıktır; konumlar her vagonun kendi pivotuna göredir; boyutlar tam genişlik/yükseklik/derinliktir. `window_clear_opening` satırları net cam hacmini, `physics_box` satırları başlangıçtaki collider kutularını gösterir. `blocksMovement=true` cam satırında camın arkasındaki sürekli beden sınırını ifade eder. Kapı açıldığında ilgili kapı engel/panelinin kapanış durumu ayrıca değişir; CSV sağlam başlangıç düzenidir.

Model tesliminde scene gizmo/collider görünümüyle pivot, scale ve açıklıkları üst üste kontrol et. İlk bakışta sığıyor diye kabul etme: F10 ile cam ve metal, iki yönlü saldırı/kapı kırılması/onarımı ve istasyon geçişi çalıştırılır. Her yeni tür/silah namlu yüksekliği veya gövde ölçüsü değişikliği ilgili açıklık kontrolünü tekrar gerektirir. M7b turret slotları ve namlu ölçüleri aşağıda eklendi; dron/store model bağlantıları kendi aşamalarında netleştirilir.

## M7b turret model bağlantısı

Kurulum artık sabit pad kullanmaz: satın alma anındaki oyuncu X/Z konumu, vagon zemini Y=0 esas alınır. Her vagonda dört yeniden kullanılabilir havuz slotu vardır; CSV slot kapasitesi/fizik ölçülerini verir, sabit kurulum koordinatı vermez. Aktör kökü zemin +Y=0,05, scale=(1,1,1). Gövde colliderı köke göre merkez Y=0,3, tam boyut **0,22 × 0,6 × 0,22 m**; carving kutusu **0,24 × 0,7 × 0,24 m**. Model tabanı hâlâ 0,5 m çapındadır; görünüm çarpışmayı büyütmez. Sahibi turret gövdesinden yürüyebilir; zombi fiziği/nav küçük engeli kullanır. Mekanik namlu aktör kökü +Y=0,9, zemin +Y=0,95'tir.

## Tren dizilimi ve lokomotif

İlerleme yönü +X (ekranda sağ), görsel çevre −X akar. Kuyruk→baş dizilimi: wagon-a 6 kapı/10 can, b 5/20, c 5/26, d 4/32, e 4/40. Merkezler X=0/13/26/39/52. Dayanıklılık her layout assetinde ayarlanır; turretteki sayılar kapı sayısından türetilmez. En iyi wagon-e'nin sağında lokomotif, solunda wagon-d görünür; Battle modunda yaşayan katılımcılar ayrı vagonlara atanır; eleme sonrası komşu boş olabilir.

Lokomotif pivotu X=63, Y=0, Z=0 (son vagon ucundan 1 m boşlukla); gövde **8 × 2 × 4,2 m**, kapalı tavan üst kotu 2,3 m, havalandırma üstü 2,34 m. Burun +X. Bu aşamada yalnız görsel bilgisayar kontrol odasıdır: collider, Health, WagonRuntime, zombi spawnı veya NavMesh kaynağı taşımaz. Model kendi görsel köküne giydirilir; oynanabilir vagonlar listesine eklenmez.

`Replaceable turret visual` yalnız sanat çocukları taşır. `Aiming head` 360 derece Y ekseninde döner, ileri yön +Z; fizik kökü döndürülmez. Model collider/NavMeshAgent/hasar scripti eklemez. Namlu pivotu bugünkü mekanik ışının kökteki X/Z ve Y=0,9 konumuna uymalı; görünen namlu ucunu değiştirmek mekanik başlangıcı değiştirmez. Muzzle socket gelecekte değişirse ortak silah ve metal/cam kabulü birlikte güncellenir. Patlama kozmetiktir; gerçek yarıçap TurretDefinition verisidir. Kurulum/atış kapı onarımını veya geçidi kapatmamalı; model tesliminde F12 ve F10 tekrar çalıştırılır. Dron ölçüleri M7c'de eklenir.

## D32 — Dron model sözleşmesi

Tek bağımsız mekanik kök: ölçek 1, +Z ileri, +Y yukarı; oyuncunun vagondaki konumunu takip eder. Kök Y=1,95 m, salınım yalnız `Replaceable drone visual` çocuğundadır. Görsel sınır yaklaşık 0,66 × 0,288 × 0,66 m; uçuş sorgu yarıçapı 0,14 m. Dron hedef/collider/NavMeshAgent/rigidbody değildir; bedenlere takılmaz, dünya geometrisi küre taramasıyla uçuşu engeller. Sanat modeline collider, Health veya hasar scripti eklenmez.

`Mechanical muzzle` köke göre (0; −0,18; 0), normal takipte zemin Y=1,77 m. `HitscanWeapon` bu soketten gerçek ışın çıkarır. Soket görsel salınım veya rotor/head altına taşınmaz. `Aiming head` +Z namlu yönü, Y dönüşü; model namlusu aynı mekanik başlangıca uyar. Bob ±0,05 m; model değişimi silah başlangıcını, kamikaze hacmini ve hasarı değiştirmez. Efekt yarıçapı kozmetiktir; mekanik patlama DroneDefinition'dadır. Turretler mevcut varsayılan Y=0,9 m namlu kuralını korur; ortak silaha yeni isteğe bağlı soket eklenmesi bunları taşımadı.

Ölçü kaynağı `generated/drone-model-dimensions.csv`; davranış ve ayarlar DRONE.md. Model tesliminde F8 dron ve F10 cam/metal kabulü yeniden çalıştırılır.

## D33 — Bot karakter modeli

Botlar insanla aynı mekanik ölçüye sahiptir: zemin kökü, ölçek 1, +Z yüz yönü; 1,7 m boy / 0,6 m çap kapsül görseli. Aynı CharacterController, hareket alanı, Y=0,9 m varsayılan namlu ve onarım mesafeleri kullanılır. `Replaceable actor visuals` altındaki model/siluet/renk değiştirilebilir; collider, CharacterController, Health, motor, aim, gun veya repair bu görsel altına taşınmaz. Animasyon root motion ile mekanik kökü sürmez. Bot profil rengi görsel yardımcıdır; beceri, hasar veya takım collider katmanından çıkarılmaz. Ad etiketi ayrı HUD sunumudur. Model tesliminde F7 bot ve F10 geometri/atış kabulü birlikte çalıştırılır; davranış ve profil ayarları BOTS.md.

D34/D35: insan/bot kök dönüşünü PlayerMotor yönetir; görünür hedefe hızlı, hedef yoksa yürüyüş girdisi yönüne ayarlanabilir hızla döner. Boşta fiziksel itme yüz yönünü değiştirmez. Modelin ileri ekseni +Z kalır. Animator veya görsel script ikinci bir kök dönüşü yazmaz; görsel salınım/animasyon alt modelde tutulur. Kapsülün açık kapıda da vagon içinde tutulması ve dönüş ayarları [PLAYER_MOVEMENT.md](PLAYER_MOVEMENT.md) içindedir.
## Solo cephane sandığı — 2026-10-09

Yeni açılan vagonlarda tek, değiştirilebilir kozmetik görsel: vagon yerel merkezi (-3.6, 0.3, 0), boyut 0.55 × 0.5 × 0.5 m. Gameplay pickup merkezi (-3.6, 0.05, 0), yatay mesafe 0.9 m; `SoloLoot` Inspector ayarı. Görselde collider/NavMesh/hasar bileşeni yoktur; hareketi ve ateşi kesmez. Görsel model değişimi toplanma bayrağını veya mekanik merkezi değiştirmez. İlk/kuyruk vagonunda sandık yoktur; Battle'da tümü gizlidir.

## D42 — Üç vagon Hayatta Kalma ölçüsü

Sahne `Assets/Game/Scenes/SurvivalIntegration.unity`; Battle modeli/sahnesi ve eski CSV'ler değişmez. Vagon merkezleri X=0/13/26, gövde 12 × 4,2 m, +X baş yönü. Dış kapılar sırasıyla 6/5/4; mevcut cam/metal atış ve oyuncu dış sınır kuralları geçerlidir. İki bağlantının merkezi X=6,5 ve 19,5; net genişlik **3 m**, net yükseklik **2,1 m**. Bağlantı zemini 1,4 × 0,24 × 3,2 m, merkezi Y=−0,12; yan korkuluklar Z=±1,6, kalınlık 0,2 m. İç uç jambları genişliği 0,6 m, merkezi Z=±1,8; üst metal alt kotu 2,1 m. Geçit hareket hacmi 3,4 × 2 × 3 m; gövdelerle örtüşme hareket yarıçapı sıkışmasını önler. Bu açıklıklar sürekli açıktır; kapatılan kapı modeli/colliderı eklenmez. Lokomotif merkezi X=37; oynanış dışı mevcut 8 m kapalı kontrol odası.

Üretilen ölçü kaynakları `generated/survival-model-dimensions.csv` ve `generated/survival-passage-dimensions.csv`. Root/pivot/collider/nav/silah köklerini değiştirmeden yalnız sanat çocuklarını giydir. Oyuncu/zombi/dron ölçeği önceki sözleşmeyle aynıdır; daha geniş bağlantı için karakterleri ölçeklemek gerekmez. Sonradan eklenecek barricade/kapı yükseltmesi yalnız kozmetik çocuk değildir: dayanıklılık ve kayıt sözleşmesi ayrı geliştirilecektir.

Hayatta Kalma turret ölçü çıktısı ayrıca `generated/survival-turret-mount-dimensions.csv` içindedir; Battle turret CSV'si yeniden yazılmaz. Açık uçların merkezinde önceki kapalı uçtan kalan lamba/kuplör görselleri kaldırılır; geçit zemini düz ve okunur kalır.

## D43 — Seyir dekoru, uzak spawn ve kamera

Survival dış platform colliderları artık 14 m genişliktedir: merkez Z=±9,18 m, iç kenar ±2,18 m, dış kenar ±16,18 m. Vagon gövdesi/kapı/cam-metal açıklığı/ara geçit ölçüleri değişmez. Doğma ankrajlarının iki sırası Z=±14 ve ±15,2 m'dir; bunlar sabit oyun kökünde kalır. Sanat modelleri spawn kaynağı, nav yüzeyi veya kapı yerine kullanılmaz.

`Journey scenery - no physics` altında seyrek elektrik direkleri (3,5 m boy / 0,13 m kalınlık; 20 m tekrar), küçük ekipman kutuları ve işaretler vardır. Collider, Light, NavMesh veya sağlık eklenmez. Lambanın ilk sarı parçası ışık kaynağı değildir. İki `Station visuals` kopyası yalnız sunum içindir; görsel kopyaya sabit platform colliderı taşıma. Traversler ve yol görselleri hareket eder; oyun zemini/navigasyon sabittir.

Kamera tek `RunPresentation` otoritesindedir; Survival'da oyuncunun X/Z konumuna odaklanır, eğim 55° ve FOV 35° referansı korunur. Korunan sunum hacmi 13 × 2,4 × 8,2 m, merkezi Y=0,4 m; gerçek kamera uzaklığı telefon/tablet safe-area oranından hesaplanır. Sağ alt mini harita bir HUD çizimidir; sanat için ikinci dünya kamerası veya minimap colliderı eklenmez. Cam/metal raycast maskeleri ve mekanik namlu soketleri değişmez. Pencere hasarı/onarımı bu sunum parçasına dahil değildir; gelecekte ayrı mekanik sözleşmesi gerekir.

## D44 kapı güçlendirme görselleri

Survival dış kapıların tabanHP'si eşit20;4/5/6 kapı fiziksel yerleşimi değişmedi. Tahta/tel tasarım modelleri DoorReinforcementVisual altında değiştirilir: genişlik1.2m, yerel yükseklik yaklaşık0.5–1.4m, yerelZ−0.13m. Kapı metal/glass gameplay collider'ları ve hareket/nav sınırları aynı kalır. Güçlendirme kapıya ek dayanıklılık sağlar; bu ilk sürümde tahta/tel görseli mermiyi engellemez. Görsel köklere collider/obstacle/gerçek ışık eklenmez. Kırıkken görünmez, onarımda mevcut seviyenin görünümü geri gelir. Her kapıda tek paylaşılan mesh/renderer; sanat meshleri de tek kapı aşaması olarak bu kökte takılır. Pencerelerde yeni hasar/kırılma davranışı yoktur.

## D45 — mayın ve bomba atar giydirme ölçüsü

Mayın oyun kökü zemin+0.025m; silindir blokout çap0.30m, yükseklik0.09m, köke göre merkezY0.055m. Kırmızı gösterge0.075m çap, merkezY0.115m; gövde/gösterge yalnız görsel, collider/Rigidbody/NavMeshObstacle/Light yok. Modeller bu kökü ve oyuncu geçiş kuralını değiştirmemeli. Tetik yarıçapı0.65m, alan hasarı2m görsel ölçüden ayrı Mine.asset verisidir.

Bomba görsel çap0.20m; süpürme yarıçapı0.10m. Muzzle mevcut oyuncu kökü+0.9m; uçan objenin görseline collider veya hit script eklenmez. Blokout temas uçuşu düz; balistik yay/sekme bu sürümde yok. Patlama ortak32 noktalı,0.35s genişleyen görsel halka/küçük flash; yalnız sunum, hasar scripti eklenmez. Runtime Instantiate/Destroy ve gerçek Light içermez. Son modelde görsel değişebilir; hasar, havuz, katman ve kayıt bağları gameplay köklerinde kalır.