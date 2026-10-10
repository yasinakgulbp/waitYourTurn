# Karton canavarlar — D55, 2026-10-10

Kullanıcının dört maket canavar referansına göre özgün modeller yeniden yapıldı. D54'ün kutu gövde/kırmızı ceket biçimi yerine açık gri karton, kırık yüzeyler, çökük gözler, dişler, kırmızı pençe uçları ve küçük yırtıklar kullanılır. Survival'da mevcut Intro/Normal, Fast ve Tough profilleri çalışır. Boss üç başlı/dikenli bir sanat kaynağıdır; dalga, sağlık, özel saldırı ve fizik boyutu henüz kararlaştırılmadı. Battle sanat bağlantısı ayrı entegrasyondur; ortak melee düzeltmesi iki modu etkiler.

| Model | Üçgen | Görsel ölçek | Bind-pose yaklaşık boy |
|---|---:|---:|---:|
| Fast | 809 | 0,84 | 1,44 m |
| Normal / Intro | 800 | 1 | 1,71 m |
| Tough | 874 | 1,13 | 1,93 m |
| Boss kaynak modeli | 1.194 | 1,08 | 2,06 m (boynuz dahil) |

Her aktör tek mesh, tek paylaşılan malzeme, 15 kemik ve en fazla iki kemik ağırlığı kullanır. Omuz–bilek ve kalça–ayak bileği boyunca sürekli loft yüzeyi, dirsek/dizde karışan ağırlıklar vardır. Yüzler, pençeler ve kat izleri aynı mesh'e birleştirilir. Yeni büyük atlas, ışık, gölge haritası veya ragdoll eklenmedi; mevcut 64px grain paylaşılır. Fizik/NavMesh kökü ölçeklenmez.

## Kaynak ve aktarım

`ArtSource/CardZombies/CardZombies.blend` düzenlenebilir kaynak; `Tools/TrainArt/build_card_zombies.py` Blender background üreticisidir. `.blend` dört tek-mesh rig ve Idle/Walk/Attack/Hit/Death kaynak action'larını içerir. Bunlar Unity Animator klipleri değildir; `CardZombieVisual` gerçek hareket ve saldırı saatinden tür pozları üretir.

Unity: 1 birim = 1 metre; Y yukarı, yüz +Z, ayak tabanı Y≈0. Blender dönüşümü `(x,-z,y)`. Rest rotation identity; hips Y=0,80, chest Y=1,05, head Y=1,40. Kollar/bacaklar üçer kemik; parmaklar el kemiğine bağlı. Önizleme kamera/ışıkları oyuna aktarılmaz.

Installer JSON'u Mesh assetlerine çevirip yalnız görsel child'ı değiştirir. Önce/sonra mevcut collider/agent/obstacle değerleri ve pozları kontrol edilir. Okunamayan eski mesh'in native tamponu CopySerialized ile güncellenemediği için yeni mesh geçici assette serialize edilip veri dosyası yenilenir; mevcut `.meta`/GUID korunur. Vertex sayısı yeniden yükleme sonrası doğrulanır.

## Animasyon ve temas

NavMeshAgent tek dünya hareket sahibidir; root motion yok. Adım fazı gerçek yer değişiminden hesaplanır. Fast kısa/hızlı adım ve asimetrik tek pençe, Normal uzanan iki kol ve sendeleme, Tough ağır kısa adım ve iki kollu savuruş kullanır. Hazırlık, temas, takip ve geri dönüş aynı MeleeAttack windup saatini izler. İki kemikli kol çözümü pençeyi oyuncu collider yüzeyine uzatır. Hedef hayatı, menzil ve metal görüş hattı darbe anında yeniden denetlenir. Görünmeyen aktörlerde kemik poz hesabı atlanır; hareket/hit saatleri korunur.

Oyuncuya merkezden erişim Fast 0,82m / Normal 0,92m / Tough 1,02m; profil menzili daha küçükse o sınır kullanılır. Önceki 1,6m merkez mesafesi oyuncuya uygulanmaz. Yaklaşma halkası küçültüldü. Kapıların saldırı rezervasyonu ve menzili korunur. Metal kontrolü aynı linecast'tir. El collider'ları eklenmedi: yakınlık sınırı ve pençe-yüzey pozu eşleştirilir; fizik tabanlı parmak teması simüle edilmez.

Health.DamageRevision değişince gövde/baş geri tepmesi oynar; heal tepki üretmez. Ölümde savaş aktörü aynı LateUpdate'ta havuza döner, ödül bir kez ödenir. Görüntü üç slot/vagon CardCorpsePool içinde devam eder: Fast yanına döner, Normal diz çökerek arkaya devrilir, Tough öne ağır düşer. Süreler 0,9 / 1,1 / 1,25s; sonunda beden yere gömülüp kapanır. Görsellerde Health/collider/agent yok; canlı kotası ve geçiş etkilenmez. Dördüncü eşzamanlı ölüm en eski slotu kullanır. Restart temizler; ön ısınma sonrası ölüm başına Instantiate/Destroy yok.

## Kabul ve açık işler

Temas/iptal/metal/tepki/ölüm/havuz: `generated/card-contact-check.txt`. Üç türde 1,25m uzaktaki hedef hasar almaz; yakın hedef windup sonunda tek darbe alır. Kalabalık/nav/gerçek dalga ve Editor süreleri: `generated/card-zombies-check.txt`; görüntüler `card-zombies-game.png`, `card-zombies-boarded.png`. Kurulum receipt'i fizik sözleşmesinin korunmasını doğrular. Bunlar Android FPS/ısı/pil kabulü değildir.

D54 oyuncu/fener dönüşü (.20s yürüyüş/.14s nişan), blur (.70) ve boarding/departure politikası korunur. A54 animasyon+ışık ölçümü, kullanıcı hareket değerlendirmesi, oyuncu modeli ve Boss savaş tasarımı açık. URP geçişi yapılmadı; ayrı branch önerisi sürer. Özel Card shader/OnRenderImage efektinin URP karşılıkları ve ayrı kabulü gerekir.
