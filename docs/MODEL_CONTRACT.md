# Model üretimi ve oyuna bağlama sözleşmesi

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
| Yan pencere net camı | Y=0,7…1,6; net yükseklik 0,9; aşağıdaki X aralıkları |
| Pencere alt/üst metali | Y=0…0,7 / 1,6…2,1; mermiyi engeller |
| Duvar segmenti dış pencere çerçevesi | X'te 0,32 genişlik; tam duvar yüksekliğinde; uç duvarla örtüşmeyi önler |
| Aynı segmentte iki camı ayıran dikme | X'te 0,18 genişlik; tam duvar yüksekliğinde |
| Vagonlar arası mesafe | Gövde uçları arasında 1,0 |

Atış çekirdeğinin namlusu aktör kökünden Y=0,9, hedef merkezi Y=0,85'tir. Modeldeki namlu/efekt bağlantısı bu atış çizgisine uymalıdır; modelin silahını başka yüksekliğe koymak mekanik namluyu kendiliğinden değiştirmez. Büyük düşmanlar veya yüksek turret namluları için aynı tablo ve kabul kontrolleri ayrıca değerlendirilir.

## Pencerelerin X aralıkları

Her iki tarafta yalnız dış kapılar **arasında** kalan duvar segmentlerine pencere açılır. Vagon uçları ile dış kapılar arasındaki dört köşe tamamen metaldir; hiç bir yerleşimde burada cam veya atış açıklığı bulunmaz. Segmentin iki ucundan 0,32 metal ayrılır. Kalan alan, net genişliği en fazla 2,2 olan eşit camlara ve aralarında 0,18 metal dikmeye bölünür.

| Segment | Net cam merkez X / genişlik |
| --- | --- |
| Sol uç (−6…−4,525) | Tamamen metal, pencere yok |
| Sağ uç (+4,525…+6) | Tamamen metal, pencere yok |
| Orta kapı mevcutken sol ara duvar (−3,075…−0,725) | −1,9 / 1,71 |
| Orta kapı mevcutken sağ ara duvar (+0,725…+3,075) | +1,9 / 1,71 |
| Orta kapı yokken büyük ara duvar (−3,075…+3,075) | −1,896667 / 1,716667; 0 / 1,716667; +1,896667 / 1,716667 |

SixDoor iki tarafta orta kapı taşır (4 yan pencere). FiveDoor kuzey orta kapıyı kaldırır (5 pencere). FourDoor iki orta kapıyı kaldırır (6 pencere). Beşli sahnede toplam 26 yan pencere vardır. Kapı sayısı zombi giriş sayısıdır; pencere ayrı bir zombi girişi değildir. Pencere kırılması, canı veya onarımı bu kararda yoktur.

## Kesit görünümü ve bağlama

Ön/güney duvarı kameranın iç mekânı göstermesi için **görsel olarak Y=1,15'te kesilir**. Arka/kuzey duvar tamdır. Bu nedenle güney camı önizlemede yalnız Y=0,7…1,15 görünür; fiziksel açıklık iki tarafta da aynı Y=0,7…1,6'dır. Kesit, mermi veya beden kurallarını değiştirmez. Modelci güney duvar üst bölümünü ayrı, gizlenebilir parça yapmalıdır; ileride kamera kesiti uygulanınca tam model kullanılabilir. Bugün kesilmiş camın üstüne görünür metal kapak eklenmez: orada fiziksel bir metal yoktur.

`Replaceable visuals - no gameplay ownership` ve `Replaceable actor visuals - no colliders` değiştirilebilir sanat kökleridir. Camın net sınırı ile fizik açıklığı aynı olmalıdır; metal çerçeve/dikme görünümü karşılık gelen kutuyla örtüşür. Render modelinde collider, NavMeshAgent, ayrı hareket/hasar scripti veya root motion etkinleştirilmez. Kapı açılma görünümü mevcut `DoorPanels`/portal durumuna bağlanır; hasar ve yol açma işi model animasyonuna verilmez.

Cam malzemesi mermi kuralını belirlemez. Tam yan sınır `ShotTransparent` katmanında beden/NavMesh engelidir; ayrı metal kutular normal atış katmanındadır. Cam görseli collider taşımaz. Ortak `HitscanResolver` ve silah maskesi cam sınırını atlar, en yakın metal/bedene çarpar. Metal model tek parça opak colliderla bütün pencereyi kapatmamalıdır. Pompalının her saçması ayrı raydır; ince dikmenin yanından geçen saçma hedefi vurabilir, dikmeye çarpan saçma orada durur.

## Üretilen ölçü listesi ve teslim kontrolü

Blok sahnenin cam malzemesi %28 opaklıkta sade renkli camdır; bu sadece okunurluk içindir. Alpha/ışık ayarını değiştirmek atış açıklığını değiştirmez. Nihai cam shaderı/ışıklar ayrı sanat ve cihaz bütçesi değerlendirmesidir. CSV ölçüleri 0,001 birime yuvarlanır; yukarıdaki tablo ilk düzenin daha hassas uç merkezlerini de verir.

**Build Five Wagons**, `WagonLayoutDefinition` verileriyle sahneyi ve NavMesh'i yeniden kurar; ardından `docs/generated/train-model-dimensions.csv` dosyasını günceller. Bu dosya üretim çıktısıdır, elle düzeltilmez. Sayılar noktalı ondalıktır; konumlar her vagonun kendi pivotuna göredir; boyutlar tam genişlik/yükseklik/derinliktir. `window_clear_opening` satırları net cam hacmini, `physics_box` satırları başlangıçtaki collider kutularını gösterir. `blocksMovement=true` cam satırında camın arkasındaki sürekli beden sınırını ifade eder. Kapı açıldığında ilgili kapı engel/panelinin kapanış durumu ayrıca değişir; CSV sağlam başlangıç düzenidir.

Model tesliminde scene gizmo/collider görünümüyle pivot, scale ve açıklıkları üst üste kontrol et. İlk bakışta sığıyor diye kabul etme: F10 ile cam ve metal, iki yönlü saldırı/kapı kırılması/onarımı ve istasyon geçişi çalıştırılır. Her yeni tür/silah namlu yüksekliği veya gövde ölçüsü değişikliği ilgili açıklık kontrolünü tekrar gerektirir. M7b turret slotları ve namlu ölçüleri aşağıda eklendi; dron/store model bağlantıları kendi aşamalarında netleştirilir.

## M7b turret model bağlantısı

Kurulum pivotu vagon yerel koordinatındadır; `generated/turret-mount-dimensions.csv` bütün pad kimlik/konumlarını verir. Aktör kökü padin Y=0,05 üzerindedir, scale=(1,1,1). Gövde colliderı köke göre merkez Y=0,3, tam boyut 0,56 × 0,6 × 0,56 m; carving kutusu 0,6 × 0,7 × 0,6 m. Mekanik namlu aktör kökü +Y=0,9, dolayısıyla pad/vagon zemini +Y=0,95'tir. Pad çapı 0,65 m görsel işarettir, collider değildir.

`Replaceable turret visual` yalnız sanat çocukları taşır. `Aiming head` 360 derece Y ekseninde döner, ileri yön +Z; fizik kökü döndürülmez. Model collider/NavMeshAgent/hasar scripti eklemez. Namlu pivotu bugünkü mekanik ışının kökteki X/Z ve Y=0,9 konumuna uymalı; görünen namlu ucunu değiştirmek mekanik başlangıcı değiştirmez. Muzzle socket gelecekte değişirse ortak silah ve metal/cam kabulü birlikte güncellenir. Patlama kozmetiktir; gerçek yarıçap TurretDefinition verisidir. Kurulum/atış kapı onarımını veya geçidi kapatmamalı; model tesliminde F12 ve F10 tekrar çalıştırılır. Dron ölçüleri M7c'de eklenir.
